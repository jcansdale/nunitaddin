namespace NUnit.AddInRunner.Tests
{
    using System;
    using System.IO;
    using Microsoft.Win32;
    using NUnit.Framework;
    using System.Reflection;

    public class NUnitRegistryTests
    {
        [TestCase("v1.1.4322", "net-1.1")]
        [TestCase("v2.0.50727", "net-2.0")]
        public void Load(string runtimeVersion, string netPath)
        {
            Version version = new Version("2.5.666.1");
            const string nunitKeyName = @"Software\nunit.org\Nunit_Test";
            string subkey = Path.Combine(nunitKeyName, "__TEST__");

            string installDir = Path.GetFullPath("__TEST_INSTALLDIR__");
            string libPath = string.Format(@"bin\{0}\lib", netPath);
            string libDir = Path.Combine(installDir, libPath);
            Directory.CreateDirectory(libDir);

            using (RegistryKey key = Registry.CurrentUser.CreateSubKey(subkey))
            {
                key.SetValue("InstallDir", installDir);
                key.SetValue("ProductVersion", version);
            }

            try
            {
                NUnitRegistry nunitRegistry = NUnitRegistry.Load(nunitKeyName);
                NUnitInfo info = nunitRegistry.Versions[0];
                Assert.That(info.InstallDir, Is.EqualTo(installDir));
                Assert.That(info.ProductVersion, Is.EqualTo(version));
                Assert.That(info.RuntimeVersion, Is.EqualTo(runtimeVersion));
                Assert.That(info.LibDir, Is.EqualTo(libDir));
            }
            finally
            {
                Registry.CurrentUser.DeleteSubKey(subkey);
                Registry.CurrentUser.DeleteSubKey(nunitKeyName);
                Directory.Delete(installDir, true);
            }
        }

        [Test]
        public void Load_CleanRegistry()
        {
            const string nunitKeyName = @"Software\nunit.org\Nunit_UNKNOWN";
            NUnitRegistry nunitRegistry = NUnitRegistry.Load(nunitKeyName, "v2.0.50727");
            Assert.That(nunitRegistry.Versions.Length, Is.EqualTo(0));
        }

        [Test]
        public void Load_Net11()
        {
            const string nunitKeyName = @"Software\nunit.org\Nunit_UNKNOWN";
            NUnitRegistry nunitRegistry = NUnitRegistry.Load(nunitKeyName, "v1.1.4322");
            Assert.That(nunitRegistry.DefaultVersions.Length, Is.EqualTo(1));
            Assert.That(nunitRegistry.DefaultVersions[0].RuntimeVersion, Is.EqualTo("v1.1.4322"));
        }

        [Test]
        public void Load_Net20()
        {
            const string nunitKeyName = @"Software\nunit.org\Nunit_UNKNOWN";
            NUnitRegistry nunitRegistry = NUnitRegistry.Load(nunitKeyName, "v2.0.50727");
            Assert.That(nunitRegistry.DefaultVersions.Length, Is.EqualTo(2));
            Assert.That(nunitRegistry.DefaultVersions[1].RuntimeVersion, Is.EqualTo("v2.0.50727"));
        }

        [Test]
        public void Load_Net30_Beta1()
        {
            // There is no .NET 3.0 version of NUnit.
            const string nunitKeyName = @"Software\nunit.org\Nunit_UNKNOWN";
            NUnitRegistry nunitRegistry = NUnitRegistry.Load(nunitKeyName, "v4.0.20409");
            Assert.That(nunitRegistry.DefaultVersions.Length, Is.EqualTo(2));
            Assert.That(nunitRegistry.DefaultVersions[1].RuntimeVersion, Is.EqualTo("v2.0.50727"));
        }
    }
}
