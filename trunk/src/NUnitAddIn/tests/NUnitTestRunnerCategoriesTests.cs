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
    public class NUnitTestRunnerCategoriesTests
    {
        [Test]
        public void IncludeCategories_Method()
        {
            using (CategoriesContext categoriesContext = new CategoriesContext())
            {
                categoriesContext.IncludeCategories = new string[] { "A" };
                NUnitTestRunner testRunner = new NUnitTestRunner();
                MockTestListener testListener = new MockTestListener();
                Assembly assembly = Assembly.GetExecutingAssembly();
                MethodBase method = typeof(Examples.Categories.CategoriesTests).GetMethod("CategoryA");
                TestRunState result = testRunner.RunMember(testListener, assembly, method);
                Assert.AreEqual(1, testListener.TestFinishedCount, "expect tests");
                Assert.AreEqual(1, testListener.SuccessCount, "Expect tests to succeed");
                Assert.AreEqual(0, testListener.FailureCount, "Expect tests to fail");
                Assert.AreEqual(0, testListener.IgnoredCount, "Expect tests ignored");
                Assert.AreEqual(result, TestRunState.Success, "Check that tests were executed");
            }
        }

        [Test]
        public void ExcludeCategories_Method()
        {
            using (CategoriesContext categoriesContext = new CategoriesContext())
            {
                categoriesContext.ExcludeCategories = new string[] { "A" };
                NUnitTestRunner testRunner = new NUnitTestRunner();
                MockTestListener testListener = new MockTestListener();
                Assembly assembly = Assembly.GetExecutingAssembly();
                MethodBase method = typeof(Examples.Categories.CategoriesTests).GetMethod("CategoryA");
                TestRunState result = testRunner.RunMember(testListener, assembly, method);
                Assert.AreEqual(1, testListener.TestFinishedCount, "expect tests");
                Assert.AreEqual(1, testListener.SuccessCount, "Expect tests to succeed");
                Assert.AreEqual(0, testListener.FailureCount, "Expect tests to fail");
                Assert.AreEqual(0, testListener.IgnoredCount, "Expect tests ignored");
                Assert.AreEqual(result, TestRunState.Success, "Check that tests were executed");
            }
        }

        [Test]
        public void IncludeCategories_Type()
        {
            using (CategoriesContext categoriesContext = new CategoriesContext())
            {
                categoriesContext.IncludeCategories = new string[] { "A" };
                NUnitTestRunner testRunner = new NUnitTestRunner();
                MockTestListener testListener = new MockTestListener();
                Assembly assembly = Assembly.GetExecutingAssembly();
                Type type = typeof(Examples.Categories.CategoriesTests);
                TestRunState result = testRunner.RunMember(testListener, assembly, type);
                Assert.AreEqual(1, testListener.TestFinishedCount, "expect tests");
                Assert.AreEqual(1, testListener.SuccessCount, "Expect tests to succeed");
                Assert.AreEqual(0, testListener.FailureCount, "Expect tests to fail");
                Assert.AreEqual(0, testListener.IgnoredCount, "Expect tests ignored");
                Assert.AreEqual(result, TestRunState.Success, "Check that tests were executed");
            }
        }

        [Test]
        public void ExcludeCategories_Type()
        {
            using (CategoriesContext categoriesContext = new CategoriesContext())
            {
                categoriesContext.ExcludeCategories = new string[] { "A" };
                NUnitTestRunner testRunner = new NUnitTestRunner();
                MockTestListener testListener = new MockTestListener();
                Assembly assembly = Assembly.GetExecutingAssembly();
                Type type = typeof(Examples.Categories.CategoriesTests);
                TestRunState result = testRunner.RunMember(testListener, assembly, type);
                Assert.AreEqual(2, testListener.TestFinishedCount, "expect tests");
                Assert.AreEqual(1, testListener.SuccessCount, "Expect tests to succeed");
                Assert.AreEqual(1, testListener.FailureCount, "Expect tests to fail");
                Assert.AreEqual(0, testListener.IgnoredCount, "Expect tests ignored");
                Assert.AreEqual(result, TestRunState.Failure, "Check that tests were executed");
            }
        }

        [Test]
        public void IncludeCategories_Namespace()
        {
            using (CategoriesContext categoriesContext = new CategoriesContext())
            {
                categoriesContext.IncludeCategories = new string[] { "A" };
                NUnitTestRunner testRunner = new NUnitTestRunner();
                MockTestListener testListener = new MockTestListener();
                Assembly assembly = Assembly.GetExecutingAssembly();
                string ns = typeof(Examples.Categories.CategoriesTests).Namespace;
                TestRunState result = testRunner.RunNamespace(testListener, assembly, ns);
                Assert.AreEqual(1, testListener.TestFinishedCount, "expect tests");
                Assert.AreEqual(1, testListener.SuccessCount, "Expect tests to succeed");
                Assert.AreEqual(0, testListener.FailureCount, "Expect tests to fail");
                Assert.AreEqual(0, testListener.IgnoredCount, "Expect tests ignored");
                Assert.AreEqual(result, TestRunState.Success, "Check that tests were executed");
            }
        }

        [Test]
        public void ExcludeCategories_Namespace()
        {
            using (CategoriesContext categoriesContext = new CategoriesContext())
            {
                categoriesContext.ExcludeCategories = new string[] { "A" };
                NUnitTestRunner testRunner = new NUnitTestRunner();
                MockTestListener testListener = new MockTestListener();
                Assembly assembly = Assembly.GetExecutingAssembly();
                string ns = typeof(Examples.Categories.CategoriesTests).Namespace;
                TestRunState result = testRunner.RunNamespace(testListener, assembly, ns);
                Assert.AreEqual(4, testListener.TestFinishedCount, "expect tests");
                Assert.AreEqual(2, testListener.SuccessCount, "Expect tests to succeed");
                Assert.AreEqual(2, testListener.FailureCount, "Expect tests to fail");
                Assert.AreEqual(0, testListener.IgnoredCount, "Expect tests ignored");
                Assert.AreEqual(result, TestRunState.Failure, "Check that tests were executed");
            }
        }

        [Test]
        public void ExcludeCategories_Assembly()
        {
            using (CategoriesContext categoriesContext = new CategoriesContext())
            {
                categoriesContext.IncludeCategories = new string[] { "A" };
                NUnitTestRunner testRunner = new NUnitTestRunner();
                MockTestListener testListener = new MockTestListener();
                Assembly assembly = Assembly.GetExecutingAssembly();
                TestRunState result = testRunner.RunAssembly(testListener, assembly);
                Assert.AreEqual(1, testListener.TestFinishedCount, "expect tests");
                Assert.AreEqual(1, testListener.SuccessCount, "Expect tests to succeed");
                Assert.AreEqual(0, testListener.FailureCount, "Expect tests to fail");
                Assert.AreEqual(0, testListener.IgnoredCount, "Expect tests ignored");
                Assert.AreEqual(result, TestRunState.Success, "Check that tests were executed");
            }
        }

        [Test]
        public void TestFixture_IncludeCategories_Namespace()
        {
            using (CategoriesContext categoriesContext = new CategoriesContext())
            {
                categoriesContext.IncludeCategories = new string[] { "C" };
                NUnitTestRunner testRunner = new NUnitTestRunner();
                MockTestListener testListener = new MockTestListener();
                Assembly assembly = Assembly.GetExecutingAssembly();
                string ns = typeof(Examples.Categories.CategoriesTests).Namespace;
                TestRunState result = testRunner.RunNamespace(testListener, assembly, ns);
                Assert.AreEqual(2, testListener.TestFinishedCount, "expect tests");
                Assert.AreEqual(1, testListener.SuccessCount, "Expect tests to succeed");
                Assert.AreEqual(1, testListener.FailureCount, "Expect tests to fail");
                Assert.AreEqual(0, testListener.IgnoredCount, "Expect tests ignored");
                Assert.AreEqual(result, TestRunState.Failure, "Check that tests were executed");
            }
        }

        [Test]
        public void TestFixture_ExcludeCategories_Namespace()
        {
            using (CategoriesContext categoriesContext = new CategoriesContext())
            {
                categoriesContext.ExcludeCategories = new string[] { "C" };
                NUnitTestRunner testRunner = new NUnitTestRunner();
                MockTestListener testListener = new MockTestListener();
                Assembly assembly = Assembly.GetExecutingAssembly();
                string ns = typeof(Examples.Categories.CategoriesTests).Namespace;
                TestRunState result = testRunner.RunNamespace(testListener, assembly, ns);
                Assert.AreEqual(3, testListener.TestFinishedCount, "expect tests");
                Assert.AreEqual(2, testListener.SuccessCount, "Expect tests to succeed");
                Assert.AreEqual(1, testListener.FailureCount, "Expect tests to fail");
                Assert.AreEqual(0, testListener.IgnoredCount, "Expect tests ignored");
                Assert.AreEqual(result, TestRunState.Failure, "Check that tests were executed");
            }
        }

        [Test]
        public void TestFixture_IncludeCategories_Type()
        {
            using (CategoriesContext categoriesContext = new CategoriesContext())
            {
                categoriesContext.IncludeCategories = new string[] { "C" };
                NUnitTestRunner testRunner = new NUnitTestRunner();
                MockTestListener testListener = new MockTestListener();
                Assembly assembly = Assembly.GetExecutingAssembly();
                Type type = typeof(Examples.Categories.CategoryTestFixture);
                TestRunState result = testRunner.RunMember(testListener, assembly, type);
                Assert.AreEqual(2, testListener.TestFinishedCount, "expect tests");
                Assert.AreEqual(1, testListener.SuccessCount, "Expect tests to succeed");
                Assert.AreEqual(1, testListener.FailureCount, "Expect tests to fail");
                Assert.AreEqual(0, testListener.IgnoredCount, "Expect tests ignored");
                Assert.AreEqual(result, TestRunState.Failure, "Check that tests were executed");
            }
        }

        // NOTE: We don't force tests to run when targeting a test fixture.
        [Test]
        public void TestFixture_ExcludeCategories_Type()
        {
            using (CategoriesContext categoriesContext = new CategoriesContext())
            {
                categoriesContext.ExcludeCategories = new string[] { "C" };
                NUnitTestRunner testRunner = new NUnitTestRunner();
                MockTestListener testListener = new MockTestListener();
                Assembly assembly = Assembly.GetExecutingAssembly();
                Type type = typeof(Examples.Categories.CategoryTestFixture);
                TestRunState result = testRunner.RunMember(testListener, assembly, type);
                Assert.AreEqual(0, testListener.TestFinishedCount, "expect tests");
                Assert.AreEqual(0, testListener.SuccessCount, "Expect tests to succeed");
                Assert.AreEqual(0, testListener.FailureCount, "Expect tests to fail");
                Assert.AreEqual(0, testListener.IgnoredCount, "Expect tests ignored");
                Assert.AreEqual(result, TestRunState.NoTests, "Check that tests were executed");
            }
        }

        class CategoriesContext : IDisposable
        {
            string[] savedIncludeCategories;
            string[] savedExcludeCategories;

            public CategoriesContext()
            {
                this.savedIncludeCategories = IncludeCategories;
                this.savedExcludeCategories = ExcludeCategories;
                IncludeCategories = null;
                ExcludeCategories = null;
            }

            public void Dispose()
            {
                IncludeCategories = this.savedIncludeCategories;
                ExcludeCategories = this.savedExcludeCategories;
            }

            public string[] IncludeCategories
            {
                get
                {
                    return (string[])AppDomain.CurrentDomain.GetData("IncludeCategories");
                }

                set
                {
                    AppDomain.CurrentDomain.SetData("IncludeCategories", value);
                }
            }

            public string[] ExcludeCategories
            {
                get
                {
                    return (string[])AppDomain.CurrentDomain.GetData("ExcludeCategories");
                }

                set
                {
                    AppDomain.CurrentDomain.SetData("ExcludeCategories", value);
                }
            }
        }
    }

    namespace Examples
    {
        namespace Categories
        {
            [TestFixture]
            public class CategoriesTests
            {
                [Test]
                public void CategoryNone()
                {
                }

                [Test, Category("A")]
                public void CategoryA()
                {
                }

                [Test, Category("B")]
                public void CategoryB()
                {
                    Assert.Fail("CategoryB");
                }
            }

            [TestFixture, Category("C")]
            public class CategoryTestFixture
            {
                [Test]
                public void TestA()
                {
                }

                [Test]
                public void TestB()
                {
                    Assert.Fail("TestB");
                }
            }
        }
    }
}
