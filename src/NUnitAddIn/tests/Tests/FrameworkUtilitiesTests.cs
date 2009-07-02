using System.IO;

namespace NUnit.AddInRunner.Tests
{
    using System;
    using System.Reflection;
    using NUnit.Framework;

    public class FrameworkUtilitiesTests
    {
        [Test]
        public void FindInstallDir()
        {
            Version version = new Version("2.5.666.1");
            const string expectedDir = @"x:\testdir";

            NUnitRegistry nunitRegistry = new NUnitRegistry(
                new NUnitInfo[] { new NUnitInfo(expectedDir, version) });
            NUnitInfo info = FrameworkUtilities.FindInstallDir(
                nunitRegistry, version, version);
            Assert.That(info.InstallDir, Is.EqualTo(expectedDir));
            Assert.That(info.ProductVersion, Is.EqualTo(version));
        }

        [Test]
        public void FindInstallDir_MaxVersion()
        {
            Version version = new Version("2.5.666.1");
            Version version2 = new Version("2.5.666.2");
            const string expectedDir = @"x:\testdir";
            const string expectedDir2 = @"x:\testdir2";

            NUnitInfo[] versions = new NUnitInfo[] {
                new NUnitInfo(expectedDir, version),
                new NUnitInfo(expectedDir2, version2) };
            NUnitRegistry nunitRegistry = new NUnitRegistry(versions);
            NUnitInfo info = FrameworkUtilities.FindInstallDir(
                nunitRegistry, version, version);
            Assert.That(info.InstallDir, Is.EqualTo(expectedDir));
            Assert.That(info.ProductVersion, Is.EqualTo(version));
        }

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