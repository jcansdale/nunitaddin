namespace NUnit.AddInRunner
{
    using System;
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

    public class NUnitTestRunner : ITestRunner
    {
        public TestRunState RunAssembly(ITestListener testListener, Assembly assembly)
        {
            using(new LibAssemblyResolver(assembly))
            {
                return runAssembly(testListener, assembly);
            }
        }

        public TestRunState RunMember(ITestListener testListener, Assembly assembly, MemberInfo member)
        {
            using (new LibAssemblyResolver(assembly))
            {
                return runMember(member, testListener, assembly);
            }
        }

        public TestRunState RunNamespace(ITestListener testListener, Assembly assembly, string ns)
        {
            using (new LibAssemblyResolver(assembly))
            {
                return runNamespace(assembly, ns, testListener);
            }
        }

        TestRunState runMember(MemberInfo member, ITestListener testListener, Assembly assembly)
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

        private TestRunState runNamespace(Assembly assembly, string ns, ITestListener testListener)
        {
            ITestFilter filter = new NamespaceFilter(assembly, ns);
            filter = addCategoriesFilter(filter);

            return run(testListener, assembly, filter);
        }

        TestRunState runAssembly(ITestListener testListener, Assembly assembly)
        {
            ITestFilter filter = TestFilter.Empty;
            filter = addCategoriesFilter(filter);
            return run(testListener, assembly, filter);
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

                if (!isSameMetadata(testMethod.Method, this.method))
                {
                    return false;
                }

                Type baseType = testMethod.FixtureType;

                Type genericTypeDefinition = getGenericTypeDefinition(baseType);
                if(genericTypeDefinition != null)
                {
                    baseType = genericTypeDefinition;
                }

                foreach (Type type in this.types)
                {
                    if (baseType == type)
                    {
                        return true;
                    }
                }

                return false;
            }

            static readonly PropertyInfo isGenericTypeProperty = typeof(Type).GetProperty("IsGenericType");
            static readonly MethodInfo getGenericTypeDefinitionMethod = typeof(Type).GetMethod("GetGenericTypeDefinition");

            static Type getGenericTypeDefinition(Type baseType)
            {
                if(isGenericTypeProperty == null)
                {
                    return null;
                }

                bool isGenericType = (bool)isGenericTypeProperty.GetValue(baseType, null);
                if (!isGenericType) return null;

                if (getGenericTypeDefinitionMethod == null)
                {
                    return null;
                }

                Type genericTypeDefinition = (Type)getGenericTypeDefinitionMethod.Invoke(baseType, null);
                return genericTypeDefinition;

                //if (baseType.IsGenericType)
                //{
                //    baseType = baseType.GetGenericTypeDefinition();
                //}
            }
        }

        static readonly PropertyInfo metadataTokenProperty = typeof(MemberInfo).GetProperty("MetadataToken");
        static readonly PropertyInfo moduleProperty = typeof(MemberInfo).GetProperty("Module");

        static bool isSameMetadata(MethodInfo memberA, MethodInfo memberB)
        {
            if (metadataTokenProperty != null && moduleProperty != null)
            {
                int tokenA = (int) metadataTokenProperty.GetValue(memberA, null);
                int tokenB = (int) metadataTokenProperty.GetValue(memberB, null);
                if (tokenA != tokenB) return false;

                object moduleA = moduleProperty.GetValue(memberA, null);
                object moduleB = moduleProperty.GetValue(memberB, null);
                if (moduleA != moduleB) return false;

                return true;
            }

            // No need to wory about generics or 64-bit on .NET 1.x.
            return (ulong)memberA.MethodHandle.Value == (ulong)memberB.MethodHandle.Value;
        }

		class TypeFilter : SimpleNameFilter
		{
			public TypeFilter(Assembly assembly, Type type)
			{
                foreach (Type candidateType in getCandidateTypes(assembly, type))
                {
                    string name = TypeHelper.GetDisplayName(candidateType);
                    string ns = candidateType.Namespace;
                    if(ns != null)
                    {
                        name = ns + "." + name;
                    }
                    this.Add(name);
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
            // NOTE: Static classes are abstract and sealed.
            if (type.IsAbstract && !type.IsSealed)
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
                summary.Name = result.FullName;
                summary.StackTrace = StackTraceFilter.Filter(result.StackTrace);
                summary.TimeSpan = TimeSpan.FromSeconds(result.Time);
                this.testListener.TestFinished(summary);
            }

            static TDF.TestState toTestState(NUC.TestResult result)
            {
                switch(result.ResultState)
                {
                    case ResultState.Success:
                        return TDF.TestState.Passed;
                    case ResultState.Failure:   // Assert.Fail
                    case ResultState.Error:     // Exception
                        return TDF.TestState.Failed;
                    case ResultState.Ignored:
                    case ResultState.Skipped:
                    case ResultState.NotRunnable:
                    case ResultState.Inconclusive:
                        return TDF.TestState.Ignored;
                    default:
                        Trace.WriteLine("Unknown ResultState: " + result.ResultState);
                        return TDF.TestState.Ignored;
                }
            }

            public void SuiteFinished(NUC.TestResult result)
            {
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
