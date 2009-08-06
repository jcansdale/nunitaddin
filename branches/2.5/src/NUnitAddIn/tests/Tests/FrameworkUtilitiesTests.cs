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
            AssemblyName[] assemblyNames = targetAssembly.GetReferencedAssemblies();

            AssemblyName assemblyName = FrameworkUtilities.FindFrameworkAssembyName(assemblyNames);

            Version expectedVersion = typeof (TestAttribute).Assembly.GetName().Version;
            Assert.That(assemblyName.Version, Is.EqualTo(expectedVersion));
        }

        [Test]
        public void FindFrameworkAssembly_None()
        {
            Assembly targetAssembly = typeof(object).Assembly;
            AssemblyName[] assemblyNames = targetAssembly.GetReferencedAssemblies();

            AssemblyName assemblyName = FrameworkUtilities.FindFrameworkAssembyName(assemblyNames);

            Assert.That(assemblyName, Is.Null);
        }
    }
}
