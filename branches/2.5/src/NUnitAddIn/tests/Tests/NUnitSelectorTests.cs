using System;
using System.IO;
using System.Reflection;
using NUnit.Framework;

namespace NUnit.AddInRunner.Tests
{
    [TestFixture]
    public class NUnitSelectorTests
    {
        [Test]
        public void GetInfo_Default()
        {
            Version frameworkVersion = new Version("2.5.666.0");
            string runtimeVersion = "v2.0.50727";
            string expectedBaseDir = @"c:\lib\dir";
            NUnitInfo defaultVersion = new NUnitInfo(frameworkVersion, runtimeVersion, expectedBaseDir);
            NUnitRegistry registry = new NUnitRegistry(runtimeVersion, new NUnitInfo[] { defaultVersion }, new NUnitInfo[0], new NUnitInfo[0]);
            WarningMessage warningMessage = new WarningMessage();
            NUnitSelector selector = new NUnitSelector(warningMessage.Handler,
                registry, frameworkVersion, frameworkVersion, frameworkVersion);

            NUnitInfo info = selector.GetInfo(frameworkVersion);

            Assert.That(info.BaseDir, Is.EqualTo(expectedBaseDir));
            Assert.That(warningMessage.Text, Is.Null);
        }

        [Test]
        public void GetInfo_Installed()
        {
            Version minVersion = new Version("2.5.0.0");
            Version maxVersion = new Version("2.5.65536.65536");
            const string installDir = @"c:\install\dir";
            const string runtimeVersion = "v2.0.50727";
            string expectedBaseDir = Path.Combine(installDir, @"bin\net-2.0\lib");
            Version frameworkVersion = new Version("2.5.5.0");
            NUnitInfo[] versions = new NUnitInfo[] { new NUnitInfo(frameworkVersion, runtimeVersion, expectedBaseDir) };
            NUnitRegistry registry = new NUnitRegistry(runtimeVersion, new NUnitInfo[0], versions, new NUnitInfo[0]);
            WarningMessage warningMessage = new WarningMessage();
            NUnitSelector selector = new NUnitSelector(warningMessage.Handler,
                registry, minVersion, maxVersion, minVersion);

            NUnitInfo info = selector.GetInfo(frameworkVersion);

            Assert.That(info, Is.Not.Null);
            Assert.That(info.BaseDir, Is.EqualTo(expectedBaseDir));
            Assert.That(warningMessage.Text, Is.Null);
        }

        [Test]
        public void GetInfo_Installed_Exact()
        {
            Version minVersion = new Version("2.5.0.0");
            Version maxVersion = new Version("2.5.65536.65536");
            const string installDir = @"c:\install\dir";
            const string runtimeVersion = "v2.0.50727";
            string expectedBaseDir = Path.Combine(installDir, @"bin\net-2.0\lib");
            string expectedBaseDir2 = Path.Combine(installDir, @"bin\net-2.0\lib2");
            Version frameworkVersion = new Version("2.5.5.0");
            Version frameworkVersion2 = new Version("2.5.6.0");
            NUnitInfo[] versions = new NUnitInfo[]
                                       {
                                           new NUnitInfo(frameworkVersion, runtimeVersion, expectedBaseDir),
                                           new NUnitInfo(frameworkVersion2, runtimeVersion, expectedBaseDir2)
                                       };
            NUnitRegistry registry = new NUnitRegistry(runtimeVersion, new NUnitInfo[0], versions, new NUnitInfo[0]);
            WarningMessage warningMessage = new WarningMessage();
            NUnitSelector selector = new NUnitSelector(warningMessage.Handler,
                registry, minVersion, maxVersion, minVersion);

            NUnitInfo info = selector.GetInfo(frameworkVersion);

            Assert.That(info, Is.Not.Null);
            Assert.That(info.ProductVersion, Is.EqualTo(frameworkVersion));
            Assert.That(info.BaseDir, Is.EqualTo(expectedBaseDir));
            Assert.That(warningMessage.Text, Is.Null);
        }

        [Test]
        public void GetInfo_Installed_Exact_MaxRuntime()
        {
            Version minVersion = new Version("2.5.0.0");
            Version maxVersion = new Version("2.5.65536.65536");
            const string installDir = @"c:\install\dir";
            const string runtimeVersion1 = "v1.1.4322";
            const string runtimeVersion2 = "v2.0.50727";
            string expectedBaseDir = Path.Combine(installDir, @"bin\net-2.0\lib");
            Version frameworkVersion = new Version("2.5.5.0");
            NUnitInfo[] versions = new NUnitInfo[]
                                       {
                                           new NUnitInfo(frameworkVersion, runtimeVersion1, expectedBaseDir),
                                           new NUnitInfo(frameworkVersion, runtimeVersion2, expectedBaseDir)
                                       };
            NUnitRegistry registry = new NUnitRegistry(runtimeVersion2, new NUnitInfo[0], versions, new NUnitInfo[0]);
            WarningMessage warningMessage = new WarningMessage();
            NUnitSelector selector = new NUnitSelector(warningMessage.Handler,
                registry, minVersion, maxVersion, minVersion);

            NUnitInfo info = selector.GetInfo(frameworkVersion);

            Assert.That(info, Is.Not.Null);
            Assert.That(info.ProductVersion, Is.EqualTo(frameworkVersion));
            Assert.That(info.BaseDir, Is.EqualTo(expectedBaseDir));
            Assert.That(info.RuntimeVersion, Is.EqualTo(runtimeVersion2));
            Assert.That(warningMessage.Text, Is.Null);
        }

        [Test]
        public void GetInfo_Developer()
        {
            Version frameworkVersion = new Version("2.5.666.0");
            string runtimeVersion = "v2.0.50727";
            string expectedBaseDir = @"c:\lib\dir";
            NUnitInfo developerVersion = new NUnitInfo(frameworkVersion, runtimeVersion, expectedBaseDir);
            NUnitRegistry registry = new NUnitRegistry(runtimeVersion, new NUnitInfo[0], new NUnitInfo[0], new NUnitInfo[] { developerVersion });
            WarningMessage warningMessage = new WarningMessage();
            NUnitSelector selector = new NUnitSelector(warningMessage.Handler,
                registry, frameworkVersion, frameworkVersion, frameworkVersion);

            NUnitInfo info = selector.GetInfo(frameworkVersion);

            Assert.That(info.BaseDir, Is.EqualTo(expectedBaseDir));
            Assert.That(warningMessage.Text, Is.Null);
        }

        [Test]
        public void GetInfo_Incompatible()
        {
            Version minVersion = new Version("2.5.0.0");
            Version maxVersion = new Version("2.5.65536.65536");
            Version rtmVersion = new Version("2.5.0.9122");
            string installedBaseDir = @"c:\installed\lib";
            string defaultBaseDir = @"c:\default\lib";
            Version frameworkVersion = new Version("2.5.0.9015"); // Beta2
            NUnitInfo[] defaultVersions = new NUnitInfo[] { new NUnitInfo(rtmVersion, "v2.0.50727", defaultBaseDir) };
            NUnitInfo[] versions = new NUnitInfo[] { new NUnitInfo(frameworkVersion, "v2.0.50727", installedBaseDir) };
            NUnitRegistry registry = new NUnitRegistry("v2.0.50727", defaultVersions, versions, new NUnitInfo[0]);
            WarningMessage warningMessage = new WarningMessage();
            NUnitSelector selector = new NUnitSelector(warningMessage.Handler,
                registry, minVersion, maxVersion, rtmVersion);

            NUnitInfo info = selector.GetInfo(frameworkVersion);

            Assert.That(info.BaseDir, Is.EqualTo(defaultBaseDir));
            Assert.That(warningMessage.Text, Is.Not.Null);
            StringAssert.Contains(rtmVersion.ToString(), warningMessage.Text,
                "Expect message re: installing RTM version.");
        }

        [Test]
        public void GetInfo_LaterThanInstalled()
        {
            Version minVersion = new Version("2.5.0.0");
            Version maxVersion = new Version("2.5.65536.65536");
            Version rtmVersion = new Version("2.5.0.9122");
            Version frameworkVersion = new Version("2.5.10.10000");
            string installedBaseDir = @"c:\installed\lib";
            string defaultBaseDir = @"c:\installed\lib";
            string runtimeVersion = "v2.0.50727";
            NUnitInfo[] versions = new NUnitInfo[] { new NUnitInfo(rtmVersion, runtimeVersion, installedBaseDir) };
            NUnitInfo[] defaultVersions = new NUnitInfo[] { new NUnitInfo(frameworkVersion, runtimeVersion, defaultBaseDir) };
            NUnitRegistry registry = new NUnitRegistry(runtimeVersion, defaultVersions, versions, new NUnitInfo[0]);
            WarningMessage warningMessage = new WarningMessage();
            NUnitSelector selector = new NUnitSelector(warningMessage.Handler,
                registry, minVersion, maxVersion, rtmVersion);

            NUnitInfo info = selector.GetInfo(frameworkVersion);

            Assert.That(info.BaseDir, Is.EqualTo(defaultBaseDir));
            Assert.That(warningMessage.Text, Is.Not.Null);
            StringAssert.Contains(frameworkVersion.ToString(), warningMessage.Text,
                "Expect message re: installing target framework version.");
        }

        [Test]
        public void GetInfo_LaterThanDefault()
        {
            Version minVersion = new Version("2.5.0.0");
            Version maxVersion = new Version("2.5.65536.65536");
            Version rtmVersion = new Version("2.5.0.9122");
            Version frameworkVersion = new Version("2.5.100.10000");
            string runtimeVersion = "v2.0.50727";
            string expectedBaseDir = @"c:\lib\dir";
            NUnitInfo defaultVersion = new NUnitInfo(rtmVersion, runtimeVersion, expectedBaseDir);

            NUnitRegistry registry = new NUnitRegistry(runtimeVersion, new NUnitInfo[] { defaultVersion }, new NUnitInfo[0], new NUnitInfo[0]);
            WarningMessage warningMessage = new WarningMessage();
            NUnitSelector selector = new NUnitSelector(warningMessage.Handler,
                registry, minVersion, maxVersion, rtmVersion);

            NUnitInfo info = selector.GetInfo(frameworkVersion);

            Assert.That(info, Is.Not.Null);
            Assert.That(warningMessage.Text, Is.Not.Null);
            StringAssert.Contains(frameworkVersion.ToString(), warningMessage.Text,
                "Expect message re: installing target framework version.");
            StringAssert.Contains("www.testdriven.net/download", warningMessage.Text,
                "Should suggest installing TestDriven.Net.");
        }

        [Test]
        public void GetInfo_RuntimeVersion_NotInstalled()
        {
            Version minVersion = new Version("2.5.0.0");
            Version maxVersion = new Version("2.5.65536.65536");
            const string installDir = @"c:\install\dir";
            string unexpectedLibDir = Path.Combine(installDir, @"bin\net-2.0\lib");
            Version frameworkVersion = new Version("2.5.5.0");
            const string runtimeVersion = "v1.1.4322";
            NUnitInfo[] versions = new NUnitInfo[] { new NUnitInfo(frameworkVersion, "v2.0.50727", unexpectedLibDir) };
            NUnitRegistry registry = new NUnitRegistry(runtimeVersion, new NUnitInfo[0], versions, new NUnitInfo[0]);
            WarningMessage warningMessage = new WarningMessage();
            NUnitSelector selector = new NUnitSelector(warningMessage.Handler,
                registry, minVersion, maxVersion, minVersion);

            NUnitInfo info = selector.GetInfo(frameworkVersion);

            Assert.That(info, Is.Null);
            Assert.That(warningMessage.Text, Is.Not.Null);
            StringAssert.Contains(runtimeVersion, warningMessage.Text,
                "Expect message re: unknown runtime version.");
            StringAssert.Contains("www.testdriven.net/download", warningMessage.Text,
                "Should suggest installing TestDriven.Net.");
        }

        [Test]
        public void GetInfo_Net40()
        {
            Version frameworkVersion = new Version("2.5.0.0");
            const string expectedBaseDir = @"c:\install\dir";
            NUnitInfo[] versions = new NUnitInfo[] { new NUnitInfo(frameworkVersion, "v2.0.50727", expectedBaseDir) };
            NUnitRegistry registry = new NUnitRegistry("v4.0.20409", versions, new NUnitInfo[0], new NUnitInfo[0]);
            WarningMessage warningMessage = new WarningMessage();
            NUnitSelector selector = new NUnitSelector(warningMessage.Handler,
                registry, frameworkVersion, frameworkVersion, frameworkVersion);

            NUnitInfo info = selector.GetInfo(frameworkVersion);

            Assert.That(info.BaseDir, Is.EqualTo(expectedBaseDir));
            Assert.That(warningMessage.Text, Is.Null);
        }

        class WarningMessage
        {
            string text;

            public WarningMessageHandler Handler
            {
                get { return new WarningMessageHandler(warningMessage); }
            }

            public string Text
            {
                get { return text; }
            }

            void warningMessage(string text)
            {
                this.text = text;
            }
        }
    }
}
