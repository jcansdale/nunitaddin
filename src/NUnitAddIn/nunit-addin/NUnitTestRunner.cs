namespace NUnit.AddInRunner
{
    using System;
    using System.IO;
    using System.Reflection;
    using System.Diagnostics;
    using System.Collections;
    using System.Collections.Generic;

    using TestDriven.Framework;
    using TestDriven.Framework.Options;
    using TDF = TestDriven.Framework;
    using ITestListener = TestDriven.Framework.ITestListener;

    using NUnit.Framework.Internal; // should we be using this?
    using NUnit.Framework.Internal.Filters; // should we be using this?
    using NUnit.Framework.Interfaces;
    using NFI = NUnit.Framework.Interfaces;
    using NUnit.Framework.Api;

    public class NUnitTestRunner : ITestRunner
    {
        string testRunnerName;

        public TestRunState RunAssembly(ITestListener testListener, Assembly assembly)
        {
            return run(testListener, assembly, new AssemblyRun());
        }

        public TestRunState RunMember(ITestListener testListener, Assembly assembly, MemberInfo member)
        {
            return run(testListener, assembly, new MemberRun(member));
        }

        public TestRunState RunNamespace(ITestListener testListener, Assembly assembly, string ns)
        {
            return run(testListener, assembly, new NamespaceRun(ns));
        }

        interface IRun
        {
            TestRunState Run(NUnitTestRunner testRunner, ITestListener testListener, Assembly assembly);
        }

        class AssemblyRun : IRun
        {
            public TestRunState Run(NUnitTestRunner testRunner, TDF.ITestListener testListener, Assembly assembly)
            {
                //ITestFilter filter = TestFilter.Empty;

                // This ensures that explicit tests are excluded.
                var localPath = toLocalPath(assembly);
                ITestFilter filter = new FullNameFilter(localPath);

                filter = testRunner.addCategoriesFilter(filter);
                return testRunner.run(testListener, assembly, filter);
            }
        }

        class MemberRun : IRun
        {
            readonly MemberInfo member;

            public MemberRun(MemberInfo member)
            {
                this.member = member;
            }

            public TestRunState Run(NUnitTestRunner testRunner, ITestListener testListener, Assembly assembly)
            {
                MethodInfo method = member as MethodInfo;
                if (method != null)
                {
                    return testRunner.runMethod(testListener, assembly, method);
                }

                Type type = member as Type;
                if (type != null)
                {
                    return testRunner.runType(testListener, assembly, type);
                }

                return TestRunState.NoTests;
            }
        }

        class NamespaceRun : IRun
        {
            readonly string ns;

            public NamespaceRun(string ns)
            {
                this.ns = ns;
            }

            public TestRunState Run(NUnitTestRunner testRunner, ITestListener testListener, Assembly assembly)
            {
                return testRunner.runNamespace(testListener, assembly, ns);
            }
        }

        TestRunState run(ITestListener testListener, Assembly assembly, IRun run)
        {
            NUnitInfo info = findNUnit(testListener);
            if (info == null)
            {
                return TestRunState.NoTests;
            }

            testRunnerName = toFriendlyName(info.ProductVersion);
            using (new AssemblyResolver(info.BaseDir))
            {
                return run.Run(this, testListener, assembly);
            }
        }

        static string toFriendlyName(Version version)
        {
            return string.Format("NUnit {0}.{1}.{2}", version.Major, version.Minor, version.Build);
        }

        static NUnitInfo findNUnit(ITestListener testListener)
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;

            string frameworkFile = Path.Combine(baseDir, @"nunit.framework.dll");
            if (!File.Exists(frameworkFile))
            {
                testListener.WriteLine("Couldn't find: " + frameworkFile, Category.Warning);
                return null;
            }

            AssemblyName assemblyName = AssemblyName.GetAssemblyName(frameworkFile);
            Version productVersion = assemblyName.Version;

            return new NUnitInfo(productVersion, baseDir);
        }

        /*
        NUnitInfo findNUnit(ITestListener testListener, Assembly targetAssembly)
        {
            WarningMessage warningMessage = new WarningMessage(testListener);

            string assemblyFile = new Uri(targetAssembly.CodeBase).LocalPath;
            AssemblyName frameworkAssembly = FrameworkUtilities.FindFrameworkAssembyName(
                assemblyFile, targetAssembly.GetReferencedAssemblies());
            if(frameworkAssembly == null)
            {
                return null;
            }

            NUnitRegistry nunitRegistry = NUnitRegistry.Load(nunitRegistryRoot,
                AppDomain.CurrentDomain.BaseDirectory, RuntimeEnvironment.GetSystemVersion());
            NUnitSelector selector = new NUnitSelector(warningMessage.Handler, nunitRegistry,
                Constants.MinVersion, Constants.MaxVersion, Constants.RtmVersion);
            Version frameworkVersion = frameworkAssembly.Version;
            NUnitInfo info = selector.GetInfo(frameworkVersion);
            if(info == null)
            {
                warningMessage.Handler("Couldn't find NUnit runner for:");
                warningMessage.Handler(frameworkAssembly.FullName);
                return null;
            }

            return info;
        }

        class WarningMessage
        {
            readonly ITestListener testListener;

            public WarningMessage(ITestListener testListener)
            {
                this.testListener = testListener;
            }

            public WarningMessageHandler Handler
            {
                get { return new WarningMessageHandler(warningMessage); }
            }

            void warningMessage(string text)
            {
                testListener.WriteLine(text, Category.Warning);
            }
        }
        */

        TestRunState runMethod(ITestListener testListener, Assembly assembly, MethodInfo method)
        {
            ITestFilter filter = createMethodFilter(assembly, method);

            // NOTE: Don't filter by category when targeting an individual method.
            //filter = addCategoriesFilter(filter);

            return run(testListener, assembly, filter);
        }

        TestRunState runType(ITestListener testListener, Assembly assembly, Type type)
        {
            ITestFilter filter = createTypeFilter(assembly, type);
            filter = addCategoriesFilter(filter);

            TestRunState state = run(testListener, assembly, filter);

            /*
            if (state == TestRunState.NoTests)
            {
                if (!type.IsPublic && hasTestFixtureAttribute(assembly, type))
                {
                    TDF.TestResult summary = new TDF.TestResult();
                    summary.State = TestState.Ignored;
                    summary.Message = "Test fixtures must be public.";
                    summary.Name = type.FullName;
                    summary.TotalTests = 1;
                    summary.TestRunnerName = testRunnerName;
                    testListener.TestFinished(summary);
                    return TestRunState.Success;
                }
            }
            */

            return state;
        }

        TestRunState runNamespace(ITestListener testListener, Assembly assembly, string ns)
        {
            ITestFilter filter = createNamespaceFilter(assembly, ns);
            filter = addCategoriesFilter(filter);

            return run(testListener, assembly, filter);
        }

        ITestFilter addCategoriesFilter(ITestFilter filter)
        {
            string[] includeCategories = TestDrivenOptions.IncludeCategories;
            if (includeCategories != null && includeCategories.Length > 0)
            {
                List<TestFilter> categoryFilters = new List<TestFilter>();
                foreach (var includeCategory in includeCategories)
                {
                    categoryFilters.Add(new CategoryFilter(includeCategory));
                }

                if (categoryFilters.Count > 0)
                {
                    return new AndFilter(filter, new OrFilter(categoryFilters.ToArray()));
                }
            }

            string[] excludeCategories = TestDrivenOptions.ExcludeCategories;
            if (excludeCategories != null && excludeCategories.Length > 0)
            {
                List<TestFilter> categoryFilters = new List<TestFilter>();
                foreach (var excludeCategory in excludeCategories)
                {
                    categoryFilters.Add(new CategoryFilter(excludeCategory));
                }

                if (categoryFilters.Count > 0)
                {
                    return new AndFilter(filter, new NotFilter(new OrFilter(categoryFilters.ToArray())));
                }
            }

            return filter;
        }

        /*
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
        */

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
            var runner = new NUnitTestAssemblyRunner(new DefaultTestAssemblyBuilder());
            string assemblyFile = toLocalPath(assembly);
            IDictionary settings = new Hashtable();
            runner.Load(assemblyFile, settings);

            int totalTestCases = runner.CountTestCases(filter);
            if (totalTestCases == 0)
            {
                return TestRunState.NoTests;
            }

            // TODO: Do we still need this?
            // TestExecutionContext.CurrentContext.TestPackage = new TestPackage("TestDriven");   // HACK

            var listener = new ProxyEventListener(testListener, totalTestCases, testRunnerName);
            var result = runner.Run(listener, filter);
            return toTestRunState(result);

            // Add Standard Services to ServiceManager

            //ServiceManager.Services.AddService(new DomainManager());
            //ServiceManager.Services.AddService(new AddinRegistry());
            //ServiceManager.Services.AddService(new AddinManager());

            // Initialize Services
            //ServiceManager.Services.InitializeServices();

            // NOTE: This "magic" is required by CoreExtensions.Host.InitializeService.
            //AppDomain.CurrentDomain.SetData("AddinRegistry", Services.AddinRegistry);

            //if (!CoreExtensions.Host.Initialized)
            //{
            //    CoreExtensions.Host.InitializeService();
            //}

            //string assemblyFile = toLocalPath(assembly);
            //TestPackage package = new TestPackage(assemblyFile);
            //TestSuiteBuilder builder = new TestSuiteBuilder();
            //TestSuite testAssembly = builder.Build(package);
            //TestExecutionContext.CurrentContext.TestPackage = new TestPackage("TestDriven");   // HACK

            //int totalTestCases = testAssembly.CountTestCases(filter);
            //if (totalTestCases == 0)
            //{
            //    return TestRunState.NoTests;
            //}

            //EventListener listener = new ProxyEventListener(testListener, totalTestCases, testRunnerName);

            //NUC.TestResult result = testAssembly.Run(listener, filter);

            //return toTestRunState(result);
        }

        static string toLocalPath(Assembly assembly)
        {
            //return new Uri(assembly.EscapedCodeBase).LocalPath;

            string escapedCodeBase = assembly.CodeBase.Replace("#", "%23");
            return new Uri(escapedCodeBase).LocalPath;
        }

        static TestRunState toTestRunState(NFI.ITestResult result)
        {
            TestStatus status = result.ResultState.Status;
            switch (status)
            {
                case TestStatus.Failed:
                    return TestRunState.Failure;
                case TestStatus.Inconclusive:
                case TestStatus.Passed:
                case TestStatus.Skipped:
                    return TestRunState.Success;
                default:
                    throw new Exception("Unknown TestStatus: " + status);
            }
        }

        ITestFilter createMethodFilter(Assembly assembly, MethodInfo method)
        {
            //return new MethodFilter(assembly, method);
            //return new MethodNameFilter(method.Name);

            Type type = method.ReflectedType;
            var types = getCandidateTypes(assembly, type);

            List<ITestFilter> filters = new List<ITestFilter>();
            foreach (Type candidateType in types)
            {
                //string fullName = candidateType.FullName + "." + method.Name;
                //filters.Add(new FullNameFilter(fullName));

                // NOTE: This doesn't work with explicit tests.
                //var filter = new AndFilter(new ClassNameFilter(candidateType.FullName), new MethodNameFilter(method.Name));

                // NOTE: This doesn't work with generic tests.
                //var filter = new FullNameFilter(candidateType.FullName + "." + method.Name);

                // NOTE: This doesn't work with explicit and generic tests.
                // TODO: Confirm with tests!
                var filter = new OrFilter(new FullNameFilter(candidateType.FullName + "." + method.Name),
                    new AndFilter(new ClassNameFilter(candidateType.FullName), new MethodNameFilter(method.Name)));

                // NOTE: This doesn't run generic test types.
                //string fullName = candidateType.Namespace + "." + TypeHelper.GetDisplayName(candidateType) + "." + method.Name;
                //var filter = new FullNameFilter(fullName);

                filters.Add(filter);
            }

            return new OrFilter(filters.ToArray());
        }

        class MethodFilter : TestFilter
        {
            MethodInfo method;
            IList types;

            public MethodFilter(Assembly assembly, MethodInfo method)
            {
                this.method = method;
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

                if(!isSameOrBaseMethod(testMethod.Method.MethodInfo, this.method))
                {
                    return false;
                }

                // Type baseType = testMethod.FixtureType;
                // NOTE: Will this always be assocated with a type?
                Type baseType = testMethod.TypeInfo.Type;

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

            static bool isSameOrBaseMethod(MethodInfo testMethod, MethodInfo targetMethod)
            {
                while(true)
                {
                    if (isSameMetadata(testMethod, targetMethod))
                    {
                        return true;
                    }

                    MethodInfo baseMethod = testMethod.GetBaseDefinition();
                    if (baseMethod == testMethod)
                    {
                        return false;
                    }

                    testMethod = baseMethod;
                }
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

            public override TNode AddToXml(TNode parentNode, bool recursive)
            {
                throw new NotImplementedException();
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

        ITestFilter createTypeFilter(Assembly assembly, Type type)
        {
            //return new TypeFilter(assembly, type);

            var types = getCandidateTypes(assembly, type);

            List<ITestFilter> filters = new List<ITestFilter>();
            foreach (Type candidateType in types)
            {
                string fullName = candidateType.FullName;
                //filters.Add(new FullNameFilter(fullName));
                filters.Add(new ClassNameFilter(fullName));
            }

            return new OrFilter(filters.ToArray());
        }

        class TypeFilter : ITestFilter
        {
            List<Type> types = new List<Type>();

			public TypeFilter(Assembly assembly, Type type)
			{
                foreach (Type candidateType in getCandidateTypes(assembly, type))
                {
                    types.Add(candidateType);

                    //string name = TypeHelper.GetDisplayName(candidateType);
                    //string ns = candidateType.Namespace;
                    //if(ns != null)
                    //{
                    //    name = ns + "." + name;
                    //}
                    //this.Add(name);
                }
			}

            public bool Pass(ITest test)
            {
                //Type fixtureType = test.FixtureType;
                ITypeInfo typeInfo = test.TypeInfo;
                if (typeInfo == null)
                {
                    // TODO: Filter only used namespaces.
                    return true;
                }

                Type fixtureType = typeInfo.Type;

                // Skip explicit test methods in suite.
                if (test.Method != null && test.RunState == RunState.Explicit)
                {
                    return false;
                }

                foreach (Type type in types)
                {
                    if (fixtureType.IsGenericType)
                    {
                        if (fixtureType.GetGenericTypeDefinition() == type)
                        {
                            return true;
                        }
                    }
                    else
                    {
                        if (fixtureType == type)
                        {
                            return true;
                        }
                    }
                }

                return false;

                //foreach (var name in names)
                //{
                //    if (test.FullName == name || test.FullName.StartsWith(name + "."))
                //    {
                //        return true;
                //    }
                //}

                //return false;
            }

            public bool IsExplicitMatch(ITest test)
            {
                // TODO: What should we return here?
                return false;
            }

            public TNode AddToXml(TNode parentNode, bool recursive)
            {
                throw new NotImplementedException();
            }

            public TNode ToXml(bool recursive)
            {
                throw new NotImplementedException();
            }
        }

        ITestFilter createNamespaceFilter(Assembly assembly, string ns)
        {
            //return new NamespaceFilter(assembly, ns);

            return new FullNameFilter(ns);
        }

        class NamespaceFilter : SimpleNameFilter
        {
            public NamespaceFilter(Assembly assembly, string ns)
                : base(ns)
            {
            }
        }

        class SimpleNameFilter : ITestFilter
        {
            List<string> names = new List<string>();

            public SimpleNameFilter()
            {
            }

            public SimpleNameFilter(string name)
            {
                Add(name);
            }

            public void Add(string name)
            {
                names.Add(name);
            }

            public bool Pass(ITest test)
            {
                // Skip all explicit test fixtures and cases in namespace.
                if (test.RunState == RunState.Explicit)
                {
                    return false;
                }

                // HACK: Only filtering methods.
                if (test.IsSuite) return true;

                foreach(var name in names)
                {
                    if (test.FullName == name || test.FullName.StartsWith(name + "."))
                    {
                        return true;
                    }
                }

                return false;
            }

            public bool IsExplicitMatch(ITest test)
            {
                // TODO: What should we return here?
                return false;
            }

            public TNode AddToXml(TNode parentNode, bool recursive)
            {
                throw new NotImplementedException();
            }

            public TNode ToXml(bool recursive)
            {
                throw new NotImplementedException();
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

        // class ProxyEventListener : EventListener
        class ProxyEventListener : NFI.ITestListener
        {
            ITestListener testListener;
            int totalTestCases;
            string testRunnerName;

            public ProxyEventListener(ITestListener testListener, int totalTestCases, string testRunnerName)
            {
                this.testListener = testListener;
                this.totalTestCases = totalTestCases;
                this.testRunnerName = testRunnerName;
            }

            public void TestStarted(ITest test)
            {
            }

            public void TestFinished(ITestResult result)
            {
                string output = result.Output;
                if (!string.IsNullOrEmpty(output))
                {
                    this.testListener.WriteLine(trimNewLine(output), Category.Output);
                }

                if (result.Test.IsSuite)
                {
                    suiteFinnished(result);
                    return;
                }

                if (result.ResultState.Status == TestStatus.Failed)
                {
                    if (result.Message != null && result.Message.StartsWith("OneTimeSetUp:"))
                    {
                        return;
                    }
                }

                TDF.TestResult summary = new TDF.TestResult();
                summary.TotalTests = totalTestCases;
                summary.State = toTestState(result);
                summary.Message = formatMessage(result.Message);
                summary.Name = result.FullName;
                // NOTE: Do we need filtering?
                // summary.StackTrace = StackTraceFilter.Filter(result.StackTrace);
                summary.StackTrace = result.StackTrace;
                summary.TimeSpan = TimeSpan.FromSeconds(result.Duration);
                summary.TestRunnerName = testRunnerName;
                this.testListener.TestFinished(summary);
            }

            void suiteFinnished(ITestResult suiteResult)
            {
                if (suiteResult.StackTrace != null)
                {
                    // NOTE: This is hacky.
                    if (suiteResult.StackTrace.StartsWith("--TearDown"))
                    {
                        this.testListener.WriteLine(suiteResult.Message, Category.Warning);
                        this.testListener.WriteLine(suiteResult.StackTrace, Category.Warning);
                        return;
                    }

                    foreach (ITestResult result in suiteResult.Children)
                    {
                        TDF.TestResult summary = new TDF.TestResult();
                        summary.TotalTests = totalTestCases;
                        summary.State = toTestState(result);
                        summary.Message = formatMessage(result.Message);
                        summary.Name = result.FullName;
                        // NOTE: Do we need filtering?
                        // summary.StackTrace = StackTraceFilter.Filter(suiteResult.StackTrace);
                        summary.StackTrace = suiteResult.StackTrace;
                        summary.TimeSpan = TimeSpan.FromSeconds(result.Duration);
                        summary.TestRunnerName = testRunnerName;
                        this.testListener.TestFinished(summary);
                    }
                }

                return;
            }

            static string trimNewLine(string output)
            {
                if (output.EndsWith(Environment.NewLine))
                {
                    output = output.Substring(0, output.Length - Environment.NewLine.Length);
                }

                return output;
            }

            // Fix assert message formatting in NUnit 2.4 and up.
            static string formatMessage(string message)
            {
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

                return message;
            }

            static TDF.TestState toTestState(ITestResult result)
            {
                switch (result.ResultState.Status)
                {
                    case TestStatus.Passed:
                        return TDF.TestState.Passed;
                    case TestStatus.Failed:
                        if (result.ResultState.Label == "Invalid")
                        {
                            // Show invalid (i.e. bad sig) tests as ignored rather than failed.
                            return TDF.TestState.Ignored;
                        }
                        return TDF.TestState.Failed;
                    case TestStatus.Skipped:
                    case TestStatus.Inconclusive:
                        return TDF.TestState.Ignored;
                    default:
                        Trace.WriteLine("Unknown ResultState: " + result.ResultState);
                        return TDF.TestState.Ignored;
                }

                //switch (result.ResultState)
                //{
                //    case ResultState.Success:
                //        return TDF.TestState.Passed;
                //    case ResultState.Failure:   // Assert.Fail
                //    case ResultState.Error:     // Exception
                //        return TDF.TestState.Failed;
                //    case ResultState.Ignored:
                //    case ResultState.Skipped:
                //    case ResultState.NotRunnable:
                //    case ResultState.Inconclusive:
                //        return TDF.TestState.Ignored;
                //    default:
                //        Trace.WriteLine("Unknown ResultState: " + result.ResultState);
                //        return TDF.TestState.Ignored;
                //}
            }

            //public void RunFinished(Exception exception)
            //{
            //}

            // TODO: Expose in above method.
            //public void SuiteFinished(NUC.TestResult result)
            //{
            //    // NOTE: Output if we have a stack trace.
            //    if (result.StackTrace != null)
            //    {
            //        if (result.Message != null && result.Message.Length > 0)
            //        {
            //            // Don't output message when child test fails.
            //            if (result.FailureSite != FailureSite.Child)
            //            {
            //                // HACK: Output as much info as we have (no exception type).
            //                this.testListener.WriteLine("TestFixture failed: " + result.Message, Category.Warning);
            //                this.testListener.WriteLine(result.StackTrace, Category.Warning);
            //            }
            //        }
            //    }
            //}

            //public void UnhandledException(Exception exception)
            //{
            //    this.testListener.WriteLine(exception.ToString(), Category.Warning);
            //}

            //public void TestOutput(TestOutput testOutput)
            //{
            //}

            //public void RunFinished(NUC.TestResult result)
            //{
            //}

            //public void RunStarted(string name, int testCount)
            //{
            //}

            //public void SuiteStarted(TestName testName)
            //{
            //}

            //public void TestStarted(TestName testName)
            //{
            //}
        }
    }
}
