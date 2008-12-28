namespace $safeprojectname$
{
    using System;
    using NUnit.Framework;

    [TestFixture]
    public class MyTests
    {
        [Test]
        public void Test1()
        {
            Console.WriteLine("Hello, World!");
        }

        [Test]
        public void Test2()
        {
            Assert.Fail("Boom!");
        }
    }
}
