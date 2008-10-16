namespace NUnit.AddInRunner
{
    using System;
    using System.Text;
    using System.Reflection;
    using System.Diagnostics;
    using System.Collections;
    using TestDriven.Framework;
    using TestDriven.Framework.Options;
    using TDF = TestDriven.Framework;
    using NUnit.Core;
    using NUC = NUnit.Core;
    using NUnit.Util;
	using NUnit.Core.Filters;
    using NUnit.Core.Builders;

    public class NUnitTestRunner : ITestRunner
    {
        // TODO: Always run test fixture excluded by category when targeted directly.

        public TestRunState RunAssembly(ITestListener testListener, Assembly assembly)
        {
            ITestFilter filter = TestFilter.Empty;
            filter = addCategoriesFilter(filter);
            return run(testListener, assembly, filter);
        }

        public TestRunState RunMember(ITestListener testListener, Assembly assembly, MemberInfo member)
        {
            MethodInfo method = member as MethodInfo;
            if (method != null)
            {
                return runMethod(testListener, assembly, method);
            }

            Type type = member as Type;
            if (type != null)
            {
                return runType(testListener, assembly, type);
            }

            return TestRunState.NoTests;
        }

        TestRunState runMethod(ITestListener testListener, Assembly assembly, MethodInfo method)
        {
            ITestFilter filter = new MethodFilter(assembly, method);

            // NOTE: Don't filter by category when targeting an individual method.
            //filter = addCategoriesFilter(filter);

            TestRunState state = run(testListener, assembly, filter);
            if (state == TestRunState.NoTests)
            {
                if (!hasTestAttribute(assembly, method))
                {
                    return TestRunState.NoTests;
                }

                if (!method.IsPublic)
                {
                    TDF.TestResult summary = new TDF.TestResult();
                    summary.State = TestState.Ignored;
                    summary.Message = "Test methods must be public.";
                    summary.Name = method.ReflectedType.FullName + "." + method.Name;
                    summary.TotalTests = 1;
                    testListener.TestFinished(summary);
                    return TestRunState.Success;
                }

                Type fixtureType = method.ReflectedType;
                if (!hasTestFixtureAttribute(assembly, fixtureType))
                {
                    TDF.TestResult summary = new TDF.TestResult();
                    summary.State = TestState.Ignored;
                    summary.Message = "No fixture attribute on parent class.";
                    summary.Name = method.ReflectedType.FullName + "." + method.Name;
                    summary.TotalTests = 1;
                    testListener.TestFinished(summary);
                    return TestRunState.Success;
                }

                if (!fixtureType.IsPublic)
                {
                    TDF.TestResult summary = new TDF.TestResult();
                    summary.State = TestState.Ignored;
                    summary.Message = "Parent fixture must be public.";
                    summary.Name = method.ReflectedType.FullName + "." + method.Name;
                    summary.TotalTests = 1;
                    testListener.TestFinished(summary);
                    return TestRunState.Success;
                }
            }

            return state;
        }

        TestRunState runType(ITestListener testListener, Assembly assembly, Type type)
        {
            ITestFilter filter = new TypeFilter(assembly, type);
            filter = addCategoriesFilter(filter);

            TestRunState state = run(testListener, assembly, filter);
            if (state == TestRunState.NoTests)
            {
                if (!type.IsPublic && hasTestFixtureAttribute(assembly, type))
                {
                    TDF.TestResult summary = new TDF.TestResult();
                    summary.State = TestState.Ignored;
                    summary.Message = "Test fixtures must be public.";
                    summary.Name = type.FullName;
                    summary.TotalTests = 1;
                    testListener.TestFinished(summary);
                    return TestRunState.Success;
                }
            }

            return state;
        }

        ITestFilter addCategoriesFilter(ITestFilter filter)
        {
            string[] includeCategories = TestDrivenOptions.IncludeCategories;
            if (includeCategories != null)
            {
                filter = new AndFilter(filter, new CategoryFilter(includeCategories));
            }

            string[] excludeCategories = TestDrivenOptions.ExcludeCategories;
            if (excludeCategories != null)
            {
                filter = new AndFilter(filter, new NotFilter(new CategoryFilter(excludeCategories)));
            }

            return filter;
        }

        static bool hasTestAttribute(Assembly assembly, MethodInfo method)
        {
            Assembly frameworkAssembly = findReferencedAssembly(assembly, "nunit.framework");
            if (frameworkAssembly == null)
            {
                return false;
            }

            Type testAttributeType = frameworkAssembly.GetType("NUnit.Framework.TestAttribute", false);
            if (testAttributeType == null)
            {
                return false;
            }

            object[] attributes = method.GetCustomAttributes(testAttributeType, true);
            if (attributes == null || attributes.Length == 0)
            {
                return false;
            }

            return true;
        }

        static bool hasTestFixtureAttribute(Assembly assembly, Type type)
        {
            Assembly frameworkAssembly = findReferencedAssembly(assembly, "nunit.framework");
            if (frameworkAssembly == null)
            {
                return false;
            }

            Type testAttributeType = frameworkAssembly.GetType("NUnit.Framework.TestFixtureAttribute", false);
            if (testAttributeType == null)
            {
                return false;
            }

            object[] attributes = type.GetCustomAttributes(testAttributeType, true);
            if (attributes == null || attributes.Length == 0)
            {
                return false;
            }

            return true;
        }

        static Assembly findReferencedAssembly(Assembly targetAssembly, string name)
        {
            foreach (AssemblyName assemblyName in targetAssembly.GetReferencedAssemblies())
            {
                if (assemblyName.Name.ToLower() == name)
                {
                    return Assembly.Load(assemblyName);
                }
            }

            return null;
        }

        public TestRunState RunNamespace(ITestListener testListener, Assembly assembly, string ns)
        {
            ITestFilter filter = new NamespaceFilter(assembly, ns);
            filter = addCategoriesFilter(filter);

            return run(testListener, assembly, filter);
        }

        TestRunState run(ITestListener testListener, Assembly assembly, ITestFilter filter)
        {
            // Add Standard Services to ServiceManager
            ServiceManager.Services.AddService(new SettingsService());
            ServiceManager.Services.AddService(new DomainManager());
            ServiceManager.Services.AddService(new AddinRegistry());
            ServiceManager.Services.AddService(new AddinManager());

            // Initialize Services
            ServiceManager.Services.InitializeServices();

            // NOTE: This "magic" is required by CoreExtensions.Host.InitializeService.
            AppDomain.CurrentDomain.SetData("AddinRegistry", Services.AddinRegistry);

			if (!CoreExtensions.Host.Initialized)
			{
				CoreExtensions.Host.InitializeService();
			}

            string assemblyFile = toLocalPath(assembly);
			TestPackage package = new TestPackage(assemblyFile);
			TestSuiteBuilder builder = new TestSuiteBuilder();
			TestSuite testAssembly = builder.Build(package);

            int totalTestCases = testAssembly.CountTestCases(filter);
            if (totalTestCases == 0)
            {
                return TestRunState.NoTests;
            }

            EventListener listener = new ProxyEventListener(testListener, totalTestCases);

            NUC.TestResult result = testAssembly.Run(listener, filter);

            return toTestRunState(result);
        }

        static string toLocalPath(Assembly assembly)
        {
            //return new Uri(assembly.EscapedCodeBase).LocalPath;

            string escapedCodeBase = assembly.CodeBase.Replace("#", "%23");
            return new Uri(escapedCodeBase).LocalPath;
        }

        static TestRunState toTestRunState(NUC.TestResult result)
        {
            if (result.IsFailure)
            {
                return TestRunState.Failure;
            }
            else if (result.Executed)
            {
                return TestRunState.Success;
            }
            else
            {
                return TestRunState.NoTests;
            }
        }

        class MethodFilter : TestFilter
        {
            MethodInfo method;
            IList types;

            public MethodFilter(Assembly assembly, MethodInfo method)
            {
                this.method = method.GetBaseDefinition();
                Type type = method.ReflectedType;
                this.types = getCandidateTypes(assembly, type);
            }

            public override bool Match(ITest test)
            {
                // HACK: What should this be replaced by?
                /*
                NotRunnableTestCase notRunnableTestCase = test as NotRunnableTestCase;
                if(notRunnableTestCase != null)
                {
                    // NOTE: Can't match on MethodInfo so use FullName.
                    string fullName = this.method.ReflectedType.FullName + "." + this.method.Name;
                    if (notRunnableTestCase.TestName.FullName == fullName)
                    {
                        return true;
                    }
                }
                */

                TestMethod testMethod = test as TestMethod;
                if (testMethod == null)
                {
                    return false;
                }

                if (testMethod.Method.MethodHandle.Value != this.method.MethodHandle.Value)
                {
                    return false;
                }

                Type reflectedType = testMethod.Method.ReflectedType;
                foreach (Type type in this.types)
                {
                    if (reflectedType == type)
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        //class MethodFilter : SimpleNameFilter
        //{
        //    public MethodFilter(Assembly assembly, MethodInfo method)
        //    {
        //        Type type = method.ReflectedType;
        //        foreach (Type candidateType in getCandidateTypes(assembly, type))
        //        {
        //            this.Add(candidateType.FullName + "." + method.Name);

        //            /* TODO:
        //            // Support for parameterized tests.
        //            if (testFullName.StartsWith(fullName + "("))
        //            {
        //                return true;
        //            }
        //            */

        //            /* TODO
        //            // Test name might be qualified with method's type name.
        //            string qualifiedMethodName = this.method.DeclaringType.Name + "." + this.method.Name;
        //            string fullName2 = type.FullName + "." + qualifiedMethodName;
        //            if (testFullName == fullName2)
        //            {
        //                return true;
        //            }
        //            */
        //        }
        //    }
        //}

		class TypeFilter : SimpleNameFilter
		{
			public TypeFilter(Assembly assembly, Type type)
			{
                foreach (Type candidateType in getCandidateTypes(assembly, type))
                {
                    this.Add(candidateType.FullName);
                }
			}
		}

		class NamespaceFilter : SimpleNameFilter
		{
			public NamespaceFilter(Assembly assembly, string ns)
				: base(ns)
			{
			}
		}

        static IList getCandidateTypes(Assembly assembly, Type type)
        {
            ArrayList types = new ArrayList();
            if (type.IsAbstract)
            {
                foreach (Type candidateType in assembly.GetExportedTypes())
                {
                    if (type.IsAssignableFrom(candidateType) && !candidateType.IsAbstract)
                    {
                        types.Add(candidateType);
                    }
                }
                return types;
            }

            types.Add(type);
            return types;
        }

		/*
        class NoFilter : IFilter
        {
            public bool Pass(TestSuite suite)
            {
                if (suite.IsExplicit)
                {
                    return false;
                }

                if (isAbstract(suite))
                {
                    return false;
                }

                return true;
            }

            public bool Pass(TestCase test)
            {
                if (test.IsExplicit)
                {
                    return false;
                }

                return true;
            }

            public bool Exclude
            {
                get { return false; }
            }
        }

        class MethodFilter : IFilter
        {
            Assembly assembly;
            MethodInfo method;
            IList types;

            public MethodFilter(Assembly assembly, MethodInfo method)
            {
                this.assembly = assembly;
                this.method = method;
                Type type = method.ReflectedType;
                this.types = getCandidateTypes(assembly, type);
            }

            public bool Pass(TestSuite suite)
            {
                if (suite.FixtureType == null)
                {
                    return true;
                }

                foreach (Type type in types)
                {
                    if (type == suite.FixtureType)
                    {
                        return true;
                    }
                }

                return false;
            }

            public bool Pass(TestCase test)
            {
                string testFullName = test.FullName;

                foreach (Type type in this.types)
                {
                    string fullName = type.FullName + "." + this.method.Name;
                    if (testFullName == fullName)
                    {
                        return true;
                    }

                    // Support for parameterized tests.
                    if (testFullName.StartsWith(fullName + "("))
                    {
                        return true;
                    }

					// Test name might be qualified with method's type name.
					string qualifiedMethodName = this.method.DeclaringType.Name + "." + this.method.Name;
					string fullName2 = type.FullName + "." + qualifiedMethodName;
					if (testFullName == fullName2)
					{
						return true;
					}
				}

                return false;
            }

            public bool Exclude
            {
                get { return false; }
            }
        }

        class TypeFilter : IFilter
        {
            Assembly assembly;
            IList types;

            public TypeFilter(Assembly assembly, Type type)
            {
                this.assembly = assembly;
                this.types = getCandidateTypes(assembly, type);
            }

            public bool Pass(TestSuite suite)
            {
                if (suite.FixtureType == null)
                {
                    return true;
                }

                foreach (Type type in types)
                {
                    if (type == suite.FixtureType)
                    {
                        return true;
                    }
                }

                return false;
            }

            public bool Pass(TestCase test)
            {
                if (test.IsExplicit)
                {
                    return false;
                }

                return true;
            }

            public bool Exclude
            {
                get { return false; }
            }
        }

        static IList getCandidateTypes(Assembly assembly, Type type)
        {
            ArrayList types = new ArrayList();
            if (type.IsAbstract)
            {
                foreach (Type candidateType in assembly.GetExportedTypes())
                {
                    if (type.IsAssignableFrom(candidateType) && !candidateType.IsAbstract)
                    {
                        types.Add(candidateType);
                    }
                }
                return types;
            }

            types.Add(type);
            return types;
        }

        class NamespaceFilter : IFilter
        {
            Assembly assembly;
            string ns;

            public NamespaceFilter(Assembly assembly, string ns)
            {
                this.assembly = assembly;
                this.ns = ns;
            }

            public bool Pass(TestSuite suite)
            {
                if (suite.IsExplicit)
                {
                    return false;
                }

                if (isAbstract(suite))
                {
                    return false;
                }

                if (suite.FullName == assembly.FullName)
                {
                    return true;
                }

                if (this.ns == "" || this.ns.StartsWith(suite.FullName + ".") || this.ns == suite.FullName || suite.FullName.StartsWith(ns + "."))
                {
                    return true;
                }

                return false;
            }

            public bool Pass(TestCase test)
            {
                if (test.IsExplicit)
                {
                    return false;
                }

                return true;
            }

            public bool Exclude
            {
                get { return false; }
            }
        }
		*/

		static bool isAbstract(TestSuite testSuite)
        {
            if (testSuite.FixtureType == null)
            {
                return false;
            }

            return testSuite.FixtureType.IsAbstract;
        }

        class ProxyEventListener : EventListener
        {
            ITestListener testListener;
            int totalTestCases;

            public ProxyEventListener(ITestListener testListener, int totalTestCases)
            {
                this.testListener = testListener;
                this.totalTestCases = totalTestCases;
            }

			public void RunFinished(Exception exception)
			{
			}

            public void TestFinished(NUC.TestResult result)
            {
                TDF.TestResult summary = new TDF.TestResult();
                summary.TotalTests = totalTestCases;
                summary.State = toTestState(result);
                string message = result.Message;
                // HACK: Fix assert message formatting in NUnit 2.4.
                if (message != null)
                {
                    if (message.EndsWith(Environment.NewLine))
                    {
                        message = message.Substring(0, message.Length - Environment.NewLine.Length);
                    }

                    if (message.StartsWith(" "))
                    {
                        message = Environment.NewLine + message;
                    }
                }
                summary.Message = message;
                summary.Name = result.Name;
                summary.StackTrace = StackTraceFilter.Filter(result.StackTrace);
                summary.TimeSpan = TimeSpan.FromSeconds(result.Time);
                this.testListener.TestFinished(summary);
            }

            static TDF.TestState toTestState(NUC.TestResult result)
            {
                if (result.IsFailure)
                {
                    return TDF.TestState.Failed;
                }

                if (!result.Executed)
                {
                    // NOTE: Does this always mean ignored?
                    return TDF.TestState.Ignored;
                }

                if (result.IsSuccess)
                {
                    return TDF.TestState.Passed;
                }


                // NOTE: What would this mean?
                return TDF.TestState.Ignored;
            }

            public void SuiteFinished(NUC.TestResult result)
            {
                //if (result.Test.TestType == "Test Fixture" && result.IsFailure)

                // NOTE: Output if we have a stack trace.
                if (result.StackTrace != null)
                {
                    if (result.Message != null && result.Message.Length > 0)
					{
                        // Don't output message when child test fails.
                        if (result.FailureSite != FailureSite.Child)
                        {
                            // HACK: Output as much info as we have (no exception type).
                            this.testListener.WriteLine("TestFixture failed: " + result.Message, Category.Info);
                            this.testListener.WriteLine(result.StackTrace, Category.Info);
                        }
					}
                }
            }

			public void UnhandledException(Exception exception)
			{
				this.testListener.WriteLine(exception.ToString(), Category.Info);
			}

			public void TestOutput(TestOutput testOutput)
			{
			}

			public void RunFinished(NUC.TestResult result)
			{
			}

			public void RunStarted(string name, int testCount)
			{
			}

			public void SuiteStarted(TestName testName)
			{
			}

			public void TestStarted(TestName testName)
			{
			}
		}
    }
}
