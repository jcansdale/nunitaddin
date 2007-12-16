namespace NUnit.AddInRunner.Tests
{
    using System;
    using System.Text;
    using NUnit.Framework;
    using TestDriven.Framework;
    using System.Reflection;
    using System.Diagnostics;
    using System.Threading;
    using Microsoft.CSharp;
    using System.Collections;

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

        // NOTE: Fix assert message formatting in NUnit 2.4.
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
            Assert.AreEqual(1, testListener.IgnoredCount);
            Assert.AreEqual(0, testListener.FailureCount);
            Assert.AreEqual(result, TestRunState.Success);
        }

        [Test]
        public void RunMember_Ignored_Test()
        {
            NUnitTestRunner testRunner = new NUnitTestRunner();
            MockTestListener testListener = new MockTestListener();
            Assembly assembly = Assembly.GetExecutingAssembly();
            MemberInfo member = typeof(Examples.IgnoredTests);
            TestRunState result = testRunner.RunMember(testListener, assembly, member);
            Assert.AreEqual(2, testListener.TestFinishedCount, "Expect test to finnish");
            Assert.AreEqual(2, testListener.IgnoredCount, "Expect test to be ignored");
            Assert.AreEqual(result, TestRunState.Success, "Check that tests were executed");
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
            MemberInfo member = typeof(NUnit.AddInRunner.Tests.Examples.NoTests).GetField("NotAMethod");
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
            Assert.AreEqual(result, TestRunState.Failure, "Check that tests were executed");
            Assert.AreEqual(2, testListener.WriteLineCount, "Check that exception info was written");
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

        class MockTestListener : ITestListener
        {
            public int TestFinishedCount;
            public int TestResultsUrlCount;
            public int WriteLineCount;
            public int SuccessCount;
            public int FailureCount;
            public int IgnoredCount;
            ArrayList testResults = new ArrayList();

            public TestResult[] TestResults
            {
                get { return (TestResult[])testResults.ToArray(typeof(TestResult)); }
            }

            public void TestFinished(TestResult summary)
            {
                this.testResults.Add(summary);
                this.TestFinishedCount++;
                switch (summary.State)
                {
                    case TestState.Passed:
                        this.SuccessCount++;
                        break;
                    case TestState.Failed:
                        this.FailureCount++;
                        break;
                    case TestState.Ignored:
                        this.IgnoredCount++;
                        break;
                }
            }

            public void TestResultsUrl(string resultsUrl)
            {
                this.TestResultsUrlCount++;
            }

            public void WriteLine(string text, Category category)
            {
                this.WriteLineCount++;
            }
        }
    }

    namespace Examples
    {
        [TestFixture]
        public class FailTests
        {
            [Test]
            public void AreEqual_False()
            {
                Assert.AreEqual(1, 2);
            }
        }

        namespace MyNamespace
        {
            [TestFixture]
            public class Fix1
            {
                [Test]
                public void Test1()
                {
                }
            }

            [TestFixture]
            public class Fix2
            {
                [Test]
                public void Test1()
                {
                }
            }
        }

        [TestFixture]
        public class MockTestFixture
        {
            [Test]
            public void Test1()
            {
            }

            [Test]
            public void Test2()
            {
            }
        }

        [TestFixture]
        public class MockTestFixture1
        {
            [Test]
            public void Test1()
            {
            }
        }

        [TestFixture]
        public class IgnoredTests
        {
            [Test, Ignore("Ignore Me")]
            public void IgnoreAttributeTest()
            {
            }

            [Test]
            public void IgnoreAssertTest()
            {
                Assert.Ignore("Ignore Me");
            }
        }

        public class NoTests
        {
            public int NotAMethod;

            public void NoTest()
            {
            }
        }

        [TestFixture]
        public abstract class AbstractTests
        {
            [Test]
            public void Test1()
            {
            }

            [Test]
            public void Test2()
            {
            }
        }

        public class ConcreateTests1 : AbstractTests
        {
        }

        public class ConcreateTests2 : AbstractTests
        {
        }

        /// <test pass="0" fail="0" ignore="2" />
        [TestFixture]
        public class BadsigTests
        {
            /// <test pass="0" fail="0" ignore="1" />
            [Test]
            public int TestRetInt()
            {
                return 0;
            }

            /// <test pass="0" fail="0" ignore="1" />
            [Test]
            public void TestTakeInt(int i)
            {
            }
        }

        /// <test pass="0" fail="0" />
        [TestFixture]
        public class TestFixtureSetUpFail
        {
            [TestFixtureSetUp]
            public void TestFixtureSetUp()
            {
                throw new Exception("Boom!");
            }

            [Test]
            public void Test1()
            {
            }
        }

        namespace FixtureSetUpTests
        {
            /// <test pass="1" fail="0" />
            [TestFixture]
            public class Fix1
            {
                [TestFixtureSetUp]
                public void TestFixtureSetUp()
                {
                    Counters.TestFixtureSetUpCount++;
                }

                [Test]
                public void Test1()
                {
                    Assert.AreEqual(1, Counters.TestFixtureSetUpCount);
                }
            }

            /// <test pass="1" fail="0" />
            [TestFixture]
            public class Fix2
            {
                [TestFixtureSetUp]
                public void TestFixtureSetUp()
                {
                    Counters.TestFixtureSetUpCount++;
                }

                [Test]
                public void Test1()
                {
                    Assert.AreEqual(1, Counters.TestFixtureSetUpCount);
                }
            }

            class Counters
            {
                internal static int TestFixtureSetUpCount;
            }
        }
    }
}
