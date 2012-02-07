namespace NUnit.AddInRunner.Tests
{
    using System;
    using System.Threading;
    using System.Reflection;
    using TestDriven.Framework;
    using NUnit.Framework;

    public class LibAssemblyResolverTests
    {
        [Test]
        public void RunMember()
        {
            AppDomainSetup info = new AppDomainSetup();
            info.ApplicationBase = AppDomain.CurrentDomain.BaseDirectory;
            AppDomain domain = AppDomain.CreateDomain("TestDomain", null, info);
            try
            {
                Type type = typeof(RemoteRunner);
                RemoteRunner runner = (RemoteRunner)domain.CreateInstanceAndUnwrap(type.Assembly.FullName, type.FullName);
                runner.RunMember_Test();
            }
            finally
            {
                AppDomain.Unload(domain);
            }
        }

        class RemoteRunner : MarshalByRefObject
        {
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
        }
    }
}
