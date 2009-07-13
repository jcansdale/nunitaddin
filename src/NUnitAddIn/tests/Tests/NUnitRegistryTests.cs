namespace NUnit.AddInRunner.Tests
{
    using System;
    using System.IO;
    using Microsoft.Win32;
    using System.Runtime.InteropServices;
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

        [Test]
        public void LoadDeveloperVersions()
        {
            NUnitInfo[] developerVersions = NUnitRegistry.LoadDeveloperVersions("v2.0.50727",
                Directory.GetCurrentDirectory(), "nunit.config");
            Assert.That(developerVersions.Length, Is.EqualTo(2));
            Assert.That(developerVersions[0].RuntimeVersion, Is.EqualTo("v1.1.4322"));
            Assert.That(developerVersions[0].BaseDir, Is.EqualTo(Path.GetFullPath("net-1.1")));
            Assert.That(developerVersions[0].ProductVersion, Is.Not.Null);
            Assert.That(developerVersions[1].RuntimeVersion, Is.EqualTo("v2.0.50727"));
            Assert.That(developerVersions[1].BaseDir, Is.EqualTo(Path.GetFullPath("net-2.0")));
            Assert.That(developerVersions[1].ProductVersion, Is.Not.Null);
        }

        [Test]
        public void LoadDeveloperVersions_BaseDir()
        {
            string baseDir = Path.GetFullPath("__BASE_DIR__");
            Directory.CreateDirectory(baseDir);
            try
            {
                string configFile = Path.Combine(baseDir, "nunit.config");
                string xml = @"<nunit><info runtimeVersion='v2.0.50727' baseDir='.' /></nunit>";
                File.WriteAllText(configFile, xml);
                string libDir = Path.Combine(baseDir, "lib");
                Directory.CreateDirectory(libDir);
                File.Copy(@"net-2.0\lib\nunit.core.dll", Path.Combine(libDir, "nunit.core.dll"));

                NUnitInfo[] developerVersions = NUnitRegistry.LoadDeveloperVersions("v2.0.50727",
                    baseDir, "nunit.config");

                Assert.That(developerVersions.Length, Is.EqualTo(1));
                Assert.That(developerVersions[0].RuntimeVersion, Is.EqualTo("v2.0.50727"));
                Assert.That(developerVersions[0].BaseDir, Is.EqualTo(baseDir));
                Assert.That(developerVersions[0].ProductVersion, Is.Not.Null);
            }
            finally
            {
                Directory.Delete(baseDir, true);
            }
        }

        [Test]
        public void LoadDeveloperVersions_Unknown()
        {
            NUnitInfo[] developerVersions = NUnitRegistry.LoadDeveloperVersions("v2.0.50727",
                AppDomain.CurrentDomain.BaseDirectory, "__UNKNOWN__.config");
            Assert.That(developerVersions.Length, Is.EqualTo(0));
        }

        [Test]
        public void LoadDeveloperVersions_NotOnPath()
        {
            string rootDir = Path.GetPathRoot(AppDomain.CurrentDomain.BaseDirectory);
            NUnitInfo[] developerVersions = NUnitRegistry.LoadDeveloperVersions("v2.0.50727",
                rootDir, "nunit.config");
            Assert.That(developerVersions.Length, Is.EqualTo(0));
        }

        [Test]
        public void LoadDeveloperVersions_Net11()
        {
            NUnitInfo[] developerVersions = NUnitRegistry.LoadDeveloperVersions("v1.1.4322",
                AppDomain.CurrentDomain.BaseDirectory, "nunit.config");
            Assert.That(developerVersions.Length, Is.EqualTo(1));
            Assert.That(developerVersions[0].RuntimeVersion, Is.EqualTo("v1.1.4322"));
        }
    }
}
