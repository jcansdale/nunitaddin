namespace NUnit.AddInRunner.Tests
{
    using System;
    using System.IO;
    using Microsoft.Win32;
    using NUnit.Framework;

    public class NUnitRegistryTests
    {
        [TestCase("v1.1.4322", "net-1.1")]
        [TestCase("v2.0.50727", "net-2.0")]
        public void LoadInstalledVersions(string runtimeVersion, string netPath)
        {
            Version version = new Version("2.5.666.1");
            const string nunitKeyName = @"Software\nunit.org\Nunit_Test";
            string subkey = Path.Combine(nunitKeyName, "__TEST__");

            string installDir = Path.GetFullPath("__TEST_INSTALLDIR__");
            string libPath = string.Format(@"bin\{0}\lib", netPath);
            string libDir = Path.Combine(installDir, libPath);
            Directory.CreateDirectory(libDir);

            foreach(string fileName in FrameworkUtilities.RequiredLibFiles)
            {
                string file = Path.Combine(libDir, fileName);
                File.WriteAllBytes(file, new byte[0]);
            }

            using (RegistryKey key = Registry.CurrentUser.CreateSubKey(subkey))
            {
                key.SetValue("InstallDir", installDir);
                key.SetValue("ProductVersion", version);
            }

            try
            {
                NUnitInfo[] installedVersions = NUnitRegistry.LoadInstalledVersions(nunitKeyName);
                NUnitInfo info = installedVersions[0];
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

        [TestCase("net-1.1")]
        [TestCase("net-2.0")]
        public void LoadInstalledVersions_Orphan(string netPath)
        {
            Version version = new Version("2.5.666.1");
            const string nunitKeyName = @"Software\nunit.org\Nunit_Orphan";
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
                NUnitInfo[] installedVersions = NUnitRegistry.LoadInstalledVersions(nunitKeyName);
                Assert.That(installedVersions.Length, Is.EqualTo(0));
            }
            finally
            {
                Registry.CurrentUser.DeleteSubKeyTree(nunitKeyName);
                Directory.Delete(installDir, true);
            }
        }

        [Test]
        public void LoadInstalledVersions_CleanRegistry()
        {
            const string nunitKeyName = @"Software\nunit.org\Nunit_UNKNOWN";
            NUnitInfo[] installedVersions = NUnitRegistry.LoadInstalledVersions(nunitKeyName);
            Assert.That(installedVersions.Length, Is.EqualTo(0));
        }

        [Test]
        public void LoadDefaultVersions_Net10()
        {
            NUnitInfo[] defaultVersions = NUnitRegistry.LoadDefaultVersions("v1.0.3705");
            Assert.That(defaultVersions.Length, Is.EqualTo(0));
        }

        [Test]
        public void LoadDefaultVersions_Net11()
        {
            NUnitInfo[] defaultVersions = NUnitRegistry.LoadDefaultVersions("v1.1.4322");
            Assert.That(defaultVersions.Length, Is.EqualTo(1));
            Assert.That(defaultVersions[0].RuntimeVersion, Is.EqualTo("v1.1.4322"));
        }

        [Test]
        public void LoadDefaultVersions_Net20()
        {
            NUnitInfo[] defaultVersions = NUnitRegistry.LoadDefaultVersions("v2.0.50727");
            Assert.That(defaultVersions.Length, Is.EqualTo(2));
            Assert.That(defaultVersions[1].RuntimeVersion, Is.EqualTo("v2.0.50727"));
        }

        [Test]
        public void LoadDefaultVersions_Net30_Beta1()
        {
            // There is no .NET 3.0 version of NUnit.
            NUnitInfo[] defaultVersions = NUnitRegistry.LoadDefaultVersions("v4.0.20409");
            Assert.That(defaultVersions.Length, Is.EqualTo(2));
            Assert.That(defaultVersions[1].RuntimeVersion, Is.EqualTo("v2.0.50727"));
        }
    }
}
