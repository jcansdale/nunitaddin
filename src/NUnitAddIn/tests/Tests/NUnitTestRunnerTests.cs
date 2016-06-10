namespace NUnit.AddInRunner.Tests
{
    using System;
    using NUnit.Framework;
    using TestDriven.Framework;
    using System.Reflection;
    using System.Threading;
    using Examples = NUnit.AddInRunner.Examples;

    [TestFixture]
    public class NUnitTestRunnerTests
    {
        [Test]
        public void RunMember_Test()
        {
            NUnitTestRunner testRunner = new NUnitTestRunner();
            MockTestListener testListener = new MockTestListener();
            Assembly assembly = Assembly.GetExecutingAssembly();
            MemberInfo member = new ThreadStart(new Examples.MockTestFixture().Test1).Method;
            TestRunState result = testRunner.RunMember(testListener, assembly, member);
            Assert.AreEqual(1, testListener.TestFinishedCount, "Expect 1 test to finnish");
            Assert.AreEqual(1, testListener.SuccessCount, "Expect 1 test to succeed");
            Assert.AreEqual(result, TestRunState.Success, "Check that tests were executed");
        }

        [Test]
        public void RunMember_TestResult_Name()
        {
            NUnitTestRunner testRunner = new NUnitTestRunner();
            MockTestListener testListener = new MockTestListener();
            Assembly assembly = Assembly.GetExecutingAssembly();
            MemberInfo member = new ThreadStart(new Examples.MockTestFixture().Test1).Method;
            TestRunState result = testRunner.RunMember(testListener, assembly, member);
            Assert.AreEqual(1, testListener.TestFinishedCount, "Expect 1 test to finnish");
            TestResult testResult = testListener.TestResults[0];
            string testName = member.DeclaringType.FullName + "." + member.Name;
            Assert.AreEqual(testName, testResult.Name);
        }

        [Test]
        public void RunMember_ConsolWriteLine_HelloWorld()
        {
            NUnitTestRunner testRunner = new NUnitTestRunner();
            MockTestListener testListener = new MockTestListener();
            Assembly assembly = Assembly.GetExecutingAssembly();
            MemberInfo member = new ThreadStart(new Examples.ConsoleTests().HelloWorld).Method;

            TestRunState result = testRunner.RunMember(testListener, assembly, member);

            Assert.That(testListener.WriteLineCount, Is.EqualTo(1));
            Assert.That(testListener.Output, Is.EqualTo("Hello, World!" + Environment.NewLine));
        }

        [Test]
        public void RunMember_ConsolWriteLine_HelloWorld2()
        {
            NUnitTestRunner testRunner = new NUnitTestRunner();
            MockTestListener testListener = new MockTestListener();
            Assembly assembly = Assembly.GetExecutingAssembly();
            MemberInfo member = new ThreadStart(new Examples.ConsoleTests().HelloWorld2).Method;

            TestRunState result = testRunner.RunMember(testListener, assembly, member);

            Assert.That(testListener.WriteLineCount, Is.EqualTo(1));
            string hello = "Hello, World!" + Environment.NewLine;
            Assert.That(testListener.Output, Is.EqualTo(hello + hello));
        }

        [Test]
        public void RunMember_ExplicitMethod_TargetMethodRuns()
        {
            NUnitTestRunner testRunner = new NUnitTestRunner();
            MockTestListener testListener = new MockTestListener();
            Assembly assembly = Assembly.GetExecutingAssembly();
            MemberInfo member = new ThreadStart(new Examples.ExplicitNamespace.ExplicitTests().ExplicitTest).Method;

            TestRunState result = testRunner.RunMember(testListener, assembly, member);

            Assert.AreEqual(1, testListener.TestFinishedCount, "Expect 1 test to finnish");
            Assert.AreEqual(1, testListener.SuccessCount, "Expect 1 test to succeed");
            Assert.AreEqual(result, TestRunState.Success, "Check that tests were executed");
        }

        [Test]
        public void RunNamespace_ExplicitMethod_TargetNamespaceNotRun()
        {
            NUnitTestRunner testRunner = new NUnitTestRunner();
            MockTestListener testListener = new MockTestListener();
            Assembly assembly = Assembly.GetExecutingAssembly();
            string ns = typeof(Examples.ExplicitNamespace.ExplicitTests).Namespace;

            TestRunState result = testRunner.RunNamespace(testListener, assembly, ns);

            Assert.That(testListener.IgnoredCount, Is.EqualTo(2), "Expect explicit tests to be ignored");
            Assert.That(testListener.TestFinishedCount, Is.EqualTo(2), "Expect 2 ignored tests");
            Assert.That(result, Is.EqualTo(TestRunState.Success), "Expect success");
        }

        [Test]
        public void RunMember_ExplicitMethod_TargetClassNotRun()
        {
            NUnitTestRunner testRunner = new NUnitTestRunner();
            MockTestListener testListener = new MockTestListener();
            Assembly assembly = Assembly.GetExecutingAssembly();
            MemberInfo member = typeof(Examples.ExplicitNamespace.ExplicitTests);

            TestRunState result = testRunner.RunMember(testListener, assembly, member);

            Assert.That(testListener.SuccessCount, Is.EqualTo(0), "Expect no tests passed");
            Assert.That(testListener.FailureCount, Is.EqualTo(0), "Expect no tests failed");
            Assert.That(testListener.IgnoredCount, Is.EqualTo(1), "Expect 1 tests ignored");
            Assert.That(testListener.TestFinishedCount, Is.EqualTo(1), "Expect 1 tests to finnish");
            Assert.That(result, Is.EqualTo(TestRunState.Success), "Check that was success");
        }

        [Test]
        public void RunMember_ExplicitFixture_TargetClassRun()
        {
            NUnitTestRunner testRunner = new NUnitTestRunner();
            MockTestListener testListener = new MockTestListener();
            Assembly assembly = Assembly.GetExecutingAssembly();
            MemberInfo member = typeof(Examples.ExplicitNamespace.ExplicitFixture);

            TestRunState result = testRunner.RunMember(testListener, assembly, member);

            Assert.That(testListener.TestFinishedCount, Is.EqualTo(1), "Expect test to finnish");
            Assert.That(result, Is.EqualTo(TestRunState.Success), "Check that test was run");
        }

        [Test]
        public void RunMember_Test_FailWithAssert()
        {
            NUnitTestRunner testRunner = new NUnitTestRunner();
            MockTestListener testListener = new MockTestListener();
            Assembly assembly = Assembly.GetExecutingAssembly();
            MemberInfo member = new ThreadStart(new Examples.FailTestFixture().FailWithAssert).Method;
            TestRunState result = testRunner.RunMember(testListener, assembly, member);
            Assert.AreEqual(1, testListener.TestFinishedCount, "Expect 1 test to finnish");
            Assert.AreEqual(0, testListener.SuccessCount, "Expect no tests to succeed");
            Assert.AreEqual(1, testListener.FailureCount, "Expect 1 test to fail");
            Assert.AreEqual(result, TestRunState.Failure, "Check that tests were executed");
        }

        [Test]
        public void RunMember_Test_FailWithException()
        {
            NUnitTestRunner testRunner = new NUnitTestRunner();
            MockTestListener testListener = new MockTestListener();
            Assembly assembly = Assembly.GetExecutingAssembly();
            MemberInfo member = new ThreadStart(new Examples.FailTestFixture().FailWithException).Method;
            TestRunState result = testRunner.RunMember(testListener, assembly, member);
            Assert.AreEqual(1, testListener.TestFinishedCount, "Expect 1 test to finnish");
            Assert.AreEqual(0, testListener.SuccessCount, "Expect no tests to succeed");
            Assert.AreEqual(1, testListener.FailureCount, "Expect 1 test to fail");
            Assert.AreEqual(result, TestRunState.Failure, "Check that tests were executed");
        }

        [Test]
        public void RunMember_AssertMessageFormatting()
        {
            NUnitTestRunner testRunner = new NUnitTestRunner();
            MockTestListener testListener = new MockTestListener();
            Assembly assembly = Assembly.GetExecutingAssembly();
            MemberInfo member = new ThreadStart(new Examples.FailTests().AreEqual_False).Method;
            TestRunState result = testRunner.RunMember(testListener, assembly, member);
            Assert.AreEqual(1, testListener.TestFinishedCount, "Expect 1 test to finnish");
            Assert.AreEqual(1, testListener.FailureCount, "Expect 1 test to fail");
            Assert.AreEqual(result, TestRunState.Failure, "Check that tests failes");
            TestResult testResult = testListener.TestResults[0];
            string message = testResult.Message;
            StringAssert.StartsWith(Environment.NewLine, message, "Check starts with new line");
            StringAssert.EndsWith("2", message, "Check end has been trimmed");
        }

        [Test]
        public void RunMember_AbstractType_Test()
        {
            NUnitTestRunner testRunner = new NUnitTestRunner();
            MockTestListener testListener = new MockTestListener();
            Assembly assembly = Assembly.GetExecutingAssembly();
            MemberInfo member = typeof(Examples.AbstractTests);
            TestRunState result = testRunner.RunMember(testListener, assembly, member);
            Assert.AreEqual(4, testListener.TestFinishedCount, "Expect tests to finnish");
            Assert.AreEqual(4, testListener.SuccessCount, "Expect tests to succeed");
            Assert.AreEqual(result, TestRunState.Success, "Check that tests were executed");
        }

        [Test]
        public void RunMember_AbstractMethod_Test()
        {
            NUnitTestRunner testRunner = new NUnitTestRunner();
            MockTestListener testListener = new MockTestListener();
            Assembly assembly = Assembly.GetExecutingAssembly();
            MemberInfo member = typeof(Examples.AbstractTests).GetMethod("Test1");
            TestRunState result = testRunner.RunMember(testListener, assembly, member);
            Assert.AreEqual(2, testListener.TestFinishedCount, "Expect tests to finnish");
            Assert.AreEqual(2, testListener.SuccessCount, "Expect tests to succeed");
            Assert.AreEqual(result, TestRunState.Success, "Check that tests were executed");
        }

        [Test]
        public void RunMember_BadsigTests_TestRetInt()
        {
            NUnitTestRunner testRunner = new NUnitTestRunner();
            MockTestListener testListener = new MockTestListener();
            Assembly assembly = Assembly.GetExecutingAssembly();
            MemberInfo member = typeof(Examples.BadsigTests).GetMethod("TestRetInt");
            TestRunState result = testRunner.RunMember(testListener, assembly, member);
            Assert.AreEqual(1, testListener.TestFinishedCount);
            Assert.AreEqual(0, testListener.SuccessCount);
            Assert.AreEqual(0, testListener.FailureCount);
            Assert.AreEqual(1, testListener.IgnoredCount);
        }

        [Test]
        public void RunMember_Ignored_Test()
        {
            NUnitTestRunner testRunner = new NUnitTestRunner();
            MockTestListener testListener = new MockTestListener();
            Assembly assembly = Assembly.GetExecutingAssembly();
            MemberInfo member = typeof(Examples.IgnoredTests);
            TestRunState result = testRunner.RunMember(testListener, assembly, member);
            Assert.That(testListener.TestFinishedCount, Is.EqualTo(2), "Expect test to finnish");
            Assert.That(testListener.IgnoredCount, Is.EqualTo(2), "Expect test to be ignored");
            Assert.That(result, Is.EqualTo(TestRunState.Success), "Check that tests were executed");
        }

        [Test]
        public void RunMember_NoTest_Test()
        {
            NUnitTestRunner testRunner = new NUnitTestRunner();
            MockTestListener testListener = new MockTestListener();
            Assembly assembly = Assembly.GetExecutingAssembly();
            MemberInfo member = new ThreadStart(new Examples.NoTests().NoTest).Method;
            TestRunState result = testRunner.RunMember(testListener, assembly, member);
            Assert.AreEqual(0, testListener.TestFinishedCount, "Expect no tests to finnish");
            Assert.AreEqual(result, TestRunState.NoTests);
        }

        [Test]
        public void RunMember_NotAMethod_Test()
        {
            NUnitTestRunner testRunner = new NUnitTestRunner();
            MockTestListener testListener = new MockTestListener();
            Assembly assembly = Assembly.GetExecutingAssembly();
            MemberInfo member = typeof(Examples.NoTests).GetField("NotAMethod");
            TestRunState result = testRunner.RunMember(testListener, assembly, member);
            Assert.AreEqual(0, testListener.TestFinishedCount, "Expect no tests to finnish");
            Assert.AreEqual(result, TestRunState.NoTests);
        }

        [Test]
        public void RunMember_TestFixture()
        {
            NUnitTestRunner testRunner = new NUnitTestRunner();
            MockTestListener testListener = new MockTestListener();
            Assembly assembly = Assembly.GetExecutingAssembly();
            MemberInfo member = typeof(Examples.MockTestFixture);
            TestRunState result = testRunner.RunMember(testListener, assembly, member);
            Assert.AreEqual(2, testListener.TestFinishedCount, "expect 2 tests to finnish");
            Assert.AreEqual(2, testListener.SuccessCount, "Expect 2 tests to succeed");
            Assert.AreEqual(result, TestRunState.Success, "Check that tests were executed");
        }

        [Test]
        public void RunMember_TestFixtureSetUp1()
        {
            Examples.FixtureSetUpTests.Counters.TestFixtureSetUpCount = 0;
            NUnitTestRunner testRunner = new NUnitTestRunner();
            MockTestListener testListener = new MockTestListener();
            Assembly assembly = Assembly.GetExecutingAssembly();
            MemberInfo member = typeof(Examples.FixtureSetUpTests.Fix1);
            TestRunState result = testRunner.RunMember(testListener, assembly, member);
            Assert.AreEqual(1, testListener.TestFinishedCount, "expect 1 tests to finnish");
            Assert.AreEqual(1, testListener.SuccessCount, "Expect 1 tests to succeed");
            Assert.AreEqual(result, TestRunState.Success, "Check that tests were executed");
        }

        [Test]
        public void RunMember_TestFixtureSetUp2()
        {
            Examples.FixtureSetUpTests.Counters.TestFixtureSetUpCount = 0;
            NUnitTestRunner testRunner = new NUnitTestRunner();
            MockTestListener testListener = new MockTestListener();
            Assembly assembly = Assembly.GetExecutingAssembly();
            MemberInfo member = typeof(Examples.FixtureSetUpTests.Fix2);
            TestRunState result = testRunner.RunMember(testListener, assembly, member);
            Assert.AreEqual(1, testListener.TestFinishedCount, "expect 1 tests to finish");
            Assert.AreEqual(1, testListener.SuccessCount, "Expect 1 tests to succeed");
            Assert.AreEqual(result, TestRunState.Success, "Check that tests were executed");
        }

        [Test]
        public void RunMember_TestFixtureSetUpFail()
        {
            NUnitTestRunner testRunner = new NUnitTestRunner();
            MockTestListener testListener = new MockTestListener();
            Assembly assembly = Assembly.GetExecutingAssembly();
            Type fixtureType = typeof(Examples.TestFixtureSetUpFail);
            TestRunState result = testRunner.RunMember(testListener, assembly, fixtureType);
            Assert.AreEqual(1, testListener.TestFinishedCount, "expect 1 tests to finish");
            Assert.AreEqual(1, testListener.FailureCount, "Expect 1 tests to fail");
            var testResult = testListener.TestResults[0];
            Assert.That(testResult.TotalTests, Is.EqualTo(1), "Check TotalTests reported in TestResut");
            StringAssert.Contains(fixtureType.FullName, testResult.StackTrace, "Check stack trace");
            Assert.AreEqual(result, TestRunState.Failure, "Check that tests were executed");
        }

        [Test]
        public void RunMember_TestFixtureTearDownFail()
        {
            NUnitTestRunner testRunner = new NUnitTestRunner();
            MockTestListener testListener = new MockTestListener();
            Assembly assembly = Assembly.GetExecutingAssembly();
            Type fixtureType = typeof(Examples.TestFixtureTearDownFail);

            TestRunState result = testRunner.RunMember(testListener, assembly, fixtureType);

            Assert.AreEqual(1, testListener.TestFinishedCount, "expect 1 tests to finish");
            Assert.AreEqual(0, testListener.FailureCount, "Expect 0 tests to fail");
            Assert.AreEqual(1, testListener.SuccessCount, "Expect 1 test to pass");
            var testResult = testListener.TestResults[0];
            Assert.That(testResult.TotalTests, Is.EqualTo(1), "Check TotalTests reported in TestResut");
            Assert.AreEqual(TestRunState.Failure, result, "TearDown exception causes Failure");
        }

        [Test]
        public void RunNamespace() 
        {
            NUnitTestRunner testRunner = new NUnitTestRunner();
            MockTestListener testListener = new MockTestListener();
            Assembly assembly = Assembly.GetExecutingAssembly();
            string ns = typeof(Examples.MyNamespace.Fix1).Namespace;
            TestRunState result = testRunner.RunNamespace(testListener, assembly, ns);
            Assert.AreEqual(2, testListener.TestFinishedCount, "expect tests");
            Assert.AreEqual(2, testListener.SuccessCount, "Expect tests to succeed");
            Assert.AreEqual(0, testListener.IgnoredCount, "Expect tests ignored");
            Assert.AreEqual(result, TestRunState.Success, "Check that tests were executed");
        }

        [Test]
        public void RunMember_NoTestFixtureAttribute()
        {
            NUnitTestRunner testRunner = new NUnitTestRunner();
            MockTestListener testListener = new MockTestListener();
            Assembly assembly = Assembly.GetExecutingAssembly();
            Type type = typeof(Examples.NoTestFixtureAttributeTests);
            TestRunState result = testRunner.RunMember(testListener, assembly, type);
            Assert.AreEqual(1, testListener.TestFinishedCount, "Expect 1 test to finnish");
            Assert.AreEqual(1, testListener.SuccessCount, "Expect 1 test to succeed");
            Assert.AreEqual(result, TestRunState.Success, "Check that tests were executed");
        }

        [Test]
        public void RunMember_StaticFixture()
        {
            NUnitTestRunner testRunner = new NUnitTestRunner();
            MockTestListener testListener = new MockTestListener();
            Assembly assembly = Assembly.GetExecutingAssembly();
            Type type = typeof(Examples.StaticFixtureTests);
            TestRunState result = testRunner.RunMember(testListener, assembly, type);
            Assert.AreEqual(1, testListener.TestFinishedCount, "Expect 1 test to finnish");
            Assert.AreEqual(1, testListener.SuccessCount, "Expect 1 test to succeed");
            Assert.AreEqual(result, TestRunState.Success, "Check that tests were executed");
        }

        [Test]
        public void RunMember_GenericFixture_Class()
        {
            NUnitTestRunner testRunner = new NUnitTestRunner();
            MockTestListener testListener = new MockTestListener();
            Assembly assembly = Assembly.GetExecutingAssembly();
            Type type = typeof(Examples.GenericFixtureTests<>);

            TestRunState result = testRunner.RunMember(testListener, assembly, type);

            Assert.AreEqual(2, testListener.TestFinishedCount, "Expect 2 tests to finnish");
            Assert.AreEqual(1, testListener.SuccessCount, "Expect 1 test to succeed");
            Assert.AreEqual(1, testListener.FailureCount, "Expect 1 test to fail");
            Assert.AreEqual(result, TestRunState.Failure);
        }

        [Test]
        public void RunMember_GenericFixture_Method()
        {
            NUnitTestRunner testRunner = new NUnitTestRunner();
            MockTestListener testListener = new MockTestListener();
            Assembly assembly = Assembly.GetExecutingAssembly();
            Type type = typeof(Examples.GenericFixtureTests<>);
            MethodInfo method = type.GetMethod("CanAddToList");
            TestRunState result = testRunner.RunMember(testListener, assembly, method);
            Assert.AreEqual(2, testListener.TestFinishedCount, "Expect 2 tests to finnish");
            Assert.AreEqual(1, testListener.SuccessCount, "Expect 1 test to succeed");
            Assert.AreEqual(1, testListener.FailureCount, "Expect 1 test to fail");
            Assert.AreEqual(result, TestRunState.Failure);
        }

        [Test]
        public void RunMember_NoNamespaceFicture()
        {
            NUnitTestRunner testRunner = new NUnitTestRunner();
            MockTestListener testListener = new MockTestListener();
            Assembly assembly = Assembly.GetExecutingAssembly();
            Type type = typeof(NoNamespaceFixtureTests);
            TestRunState result = testRunner.RunMember(testListener, assembly, type);
            Assert.AreEqual(1, testListener.TestFinishedCount, "Expect 1 test to finnish");
            Assert.AreEqual(1, testListener.SuccessCount, "Expect 1 test to succeed");
            Assert.AreEqual(result, TestRunState.Success);
        }

        [Test]
        public void RunMember_NestedClass()
        {
            NUnitTestRunner testRunner = new NUnitTestRunner();
            MockTestListener testListener = new MockTestListener();
            Assembly assembly = Assembly.GetExecutingAssembly();
            Type type = typeof(Examples.OuterClass.NestedClassTests);
            TestRunState result = testRunner.RunMember(testListener, assembly, type);
            Assert.AreEqual(1, testListener.TestFinishedCount, "Expect 1 test to finnish");
            Assert.AreEqual(1, testListener.SuccessCount, "Expect 1 test to succeed");
            Assert.AreEqual(result, TestRunState.Success);
        }

        [Test]
        public void RunMember_ThrowException()
        {
            NUnitTestRunner testRunner = new NUnitTestRunner();
            MockTestListener testListener = new MockTestListener();
            Assembly assembly = Assembly.GetExecutingAssembly();
            Type type = typeof(Examples.ExceptionTests);
            TestRunState result = testRunner.RunMember(testListener, assembly, type);
            Assert.AreEqual(TestRunState.Failure, result);
            Assert.AreEqual(1, testListener.TestFinishedCount);
            Assert.AreEqual(1, testListener.FailureCount);
        }

        [Test]
        public void RunMember_Inconclusive()
        {
            NUnitTestRunner testRunner = new NUnitTestRunner();
            MockTestListener testListener = new MockTestListener();
            Assembly assembly = Assembly.GetExecutingAssembly();
            Type type = typeof(Examples.InconclusiveTests);
            TestRunState result = testRunner.RunMember(testListener, assembly, type);
            Assert.AreEqual(TestRunState.Success, result);
            Assert.AreEqual(1, testListener.TestFinishedCount);
            Assert.AreEqual(1, testListener.IgnoredCount);
        }

        [Test]
        public void RunMember_AssumeFalse()
        {
            NUnitTestRunner testRunner = new NUnitTestRunner();
            MockTestListener testListener = new MockTestListener();
            Assembly assembly = Assembly.GetExecutingAssembly();
            Type type = typeof(Examples.AssumeTests);
            TestRunState result = testRunner.RunMember(testListener, assembly, type);
            Assert.AreEqual(TestRunState.Success, result);
            Assert.AreEqual(1, testListener.TestFinishedCount);
            Assert.AreEqual(1, testListener.IgnoredCount);
        }

        /*
        [Test]
        public void RunMember_NoFrameworkReference()
        {
            NUnitTestRunner testRunner = new NUnitTestRunner();
            MockTestListener testListener = new MockTestListener();
            Type type = typeof(Uri);
            Assembly assembly = type.Assembly;

            TestRunState result = testRunner.RunMember(testListener, assembly, type);

            Assert.AreEqual(TestRunState.NoTests, result);
            Assert.AreEqual(0, testListener.TestFinishedCount);
        }
        */

        [Test]
        public void RunMember_OverrideTest_Success()
        {
            NUnitTestRunner testRunner = new NUnitTestRunner();
            MockTestListener testListener = new MockTestListener();
            MethodInfo method = new ThreadStart(new Examples.OverrideTests.OverrideFixture().Test).Method;
            Assembly assembly = method.DeclaringType.Assembly;

            TestRunState result = testRunner.RunMember(testListener, assembly, method);

            Assert.AreEqual(TestRunState.Success, result);
            Assert.AreEqual(1, testListener.TestFinishedCount);
        }

        [Test]
        public void RunMember_OverriddenTest_Success()
        {
            NUnitTestRunner testRunner = new NUnitTestRunner();
            MockTestListener testListener = new MockTestListener();
            MethodInfo method = typeof(Examples.OverrideTests.FixtureBase).GetMethod("Test");
            Assembly assembly = method.DeclaringType.Assembly;

            TestRunState result = testRunner.RunMember(testListener, assembly, method);

            Assert.AreEqual(TestRunState.Success, result);
            Assert.AreEqual(1, testListener.TestFinishedCount);
        }
    }
}
