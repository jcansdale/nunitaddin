namespace NUnit.AddInRunner.Tests
{
    using System;
    using System.Reflection;
    using NUnit.Framework;

    public class FrameworkUtilitiesTests
    {
        [Test]
        public void FindFrameworkAssembly()
        {
            Assembly targetAssembly = GetType().Assembly;
            Assembly assembly = FrameworkUtilities.FindFrameworkAssembly(targetAssembly);
            Assert.That(assembly, Is.EqualTo(typeof(TestAttribute).Assembly));
        }

        [Test]
        public void FindFrameworkAssembly_None()
        {
            Assembly targetAssembly = typeof(object).Assembly;
            Assembly assembly = FrameworkUtilities.FindFrameworkAssembly(targetAssembly);
            Assert.That(assembly, Is.Null);
        }
    }
}
