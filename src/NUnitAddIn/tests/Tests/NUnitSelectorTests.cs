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
        public void GetLibDir_Default()
        {
            Version frameworkVersion = new Version("2.5.666.0");
            string runtimeVersion = "v2.0.50727";
            string expectedLibDir = @"c:\lib\dir";
            NUnitInfo defaultVersion = new NUnitInfo(null, frameworkVersion, runtimeVersion, expectedLibDir);
            NUnitRegistry registry = new NUnitRegistry(runtimeVersion, new NUnitInfo[] { defaultVersion }, new NUnitInfo[0]);
            WarningMessage warningMessage = new WarningMessage();
            NUnitSelector selector = new NUnitSelector(warningMessage.Handler,
                registry, frameworkVersion, frameworkVersion, frameworkVersion);

            string libDir = selector.GetLibDir(frameworkVersion);

            Assert.That(libDir, Is.EqualTo(expectedLibDir));
            Assert.That(warningMessage.Text, Is.Null);
        }

        [Test]
        public void GetLibDir_Installed()
        {
            Version minVersion = new Version("2.5.0.0");
            Version maxVersion = new Version("2.5.65536.65536");
            const string installDir = @"c:\install\dir";
            const string runtimeVersion = "v2.0.50727";
            string expectedLibDir = Path.Combine(installDir, @"bin\net-2.0\lib");
            Version frameworkVersion = new Version("2.5.5.0");
            NUnitInfo[] versions = new NUnitInfo[] { new NUnitInfo(installDir, frameworkVersion, runtimeVersion, expectedLibDir) };
            NUnitRegistry registry = new NUnitRegistry(runtimeVersion, new NUnitInfo[0], versions);
            WarningMessage warningMessage = new WarningMessage();
            NUnitSelector selector = new NUnitSelector(warningMessage.Handler,
                registry, minVersion, maxVersion, minVersion);

            string libDir = selector.GetLibDir(frameworkVersion);

            Assert.That(libDir, Is.Not.Null);
            Assert.That(libDir, Is.EqualTo(expectedLibDir));
            Assert.That(warningMessage.Text, Is.Null);
        }

        [Test]
        public void GetLibDir_Incompatible()
        {
            Version minVersion = new Version("2.5.0.0");
            Version maxVersion = new Version("2.5.65536.65536");
            Version rtmVersion = new Version("2.5.0.9122");
            string installedLibDir = @"c:\installed\lib";
            string defaultLibDir = @"c:\default\lib";
            Version frameworkVersion = new Version("2.5.0.9015"); // Beta2
            NUnitInfo[] defaultVersions = new NUnitInfo[] { new NUnitInfo(null, rtmVersion, "v2.0.50727", defaultLibDir) };
            NUnitInfo[] versions = new NUnitInfo[] { new NUnitInfo(null, frameworkVersion, "v2.0.50727", installedLibDir) };
            NUnitRegistry registry = new NUnitRegistry("v2.0.50727", defaultVersions, versions);
            WarningMessage warningMessage = new WarningMessage();
            NUnitSelector selector = new NUnitSelector(warningMessage.Handler,
                registry, minVersion, maxVersion, rtmVersion);

            string libDir = selector.GetLibDir(frameworkVersion);

            Assert.That(libDir, Is.EqualTo(defaultLibDir));
            Assert.That(warningMessage.Text, Is.Not.Null);
            StringAssert.Contains(rtmVersion.ToString(), warningMessage.Text,
                "Expect message re: installing RTM version.");
        }

        [Test]
        public void GetLibDir_LaterThanInstalled()
        {
            Version minVersion = new Version("2.5.0.0");
            Version maxVersion = new Version("2.5.65536.65536");
            Version rtmVersion = new Version("2.5.0.9122");
            Version frameworkVersion = new Version("2.5.10.10000");
            string installedLibDir = @"c:\installed\lib";
            string defaultLibDir = @"c:\installed\lib";
            string runtimeVersion = "v2.0.50727";
            NUnitInfo[] versions = new NUnitInfo[] { new NUnitInfo(null, rtmVersion, runtimeVersion, installedLibDir) };
            NUnitInfo[] defaultVersions = new NUnitInfo[] { new NUnitInfo(null, frameworkVersion, runtimeVersion, defaultLibDir) };
            NUnitRegistry registry = new NUnitRegistry(runtimeVersion, defaultVersions, versions);
            WarningMessage warningMessage = new WarningMessage();
            NUnitSelector selector = new NUnitSelector(warningMessage.Handler,
                registry, minVersion, maxVersion, rtmVersion);

            string libDir = selector.GetLibDir(frameworkVersion);

            Assert.That(libDir, Is.EqualTo(defaultLibDir));
            Assert.That(warningMessage.Text, Is.Not.Null);
            StringAssert.Contains(frameworkVersion.ToString(), warningMessage.Text,
                "Expect message re: installing target framework version.");
        }

        [Test]
        public void GetLibDir_LaterThanDefault()
        {
            Version minVersion = new Version("2.5.0.0");
            Version maxVersion = new Version("2.5.65536.65536");
            Version rtmVersion = new Version("2.5.0.9122");
            Version frameworkVersion = new Version("2.5.100.10000");
            string runtimeVersion = "v2.0.50727";
            string expectedLibDir = @"c:\lib\dir";
            NUnitInfo defaultVersion = new NUnitInfo(null, rtmVersion, runtimeVersion, expectedLibDir);

            NUnitRegistry registry = new NUnitRegistry(runtimeVersion, new NUnitInfo[] { defaultVersion }, new NUnitInfo[0]);
            WarningMessage warningMessage = new WarningMessage();
            NUnitSelector selector = new NUnitSelector(warningMessage.Handler,
                registry, minVersion, maxVersion, rtmVersion);

            string libDir = selector.GetLibDir(frameworkVersion);

            Assert.That(libDir, Is.Not.Null);
            Assert.That(warningMessage.Text, Is.Not.Null);
            StringAssert.Contains(frameworkVersion.ToString(), warningMessage.Text,
                "Expect message re: installing target framework version.");
            StringAssert.Contains("www.testdriven.net/download", warningMessage.Text,
                "Should suggest installing TestDriven.Net.");
        }

        [Test]
        public void GetLibDir_RuntimeVersion_NotInstalled()
        {
            Version minVersion = new Version("2.5.0.0");
            Version maxVersion = new Version("2.5.65536.65536");
            const string installDir = @"c:\install\dir";
            string unexpectedLibDir = Path.Combine(installDir, @"bin\net-2.0\lib");
            Version frameworkVersion = new Version("2.5.5.0");
            string runtimeVersion = "v1.1.4322";
            NUnitInfo[] versions = new NUnitInfo[] { new NUnitInfo(installDir, frameworkVersion, "v2.0.50727", unexpectedLibDir) };
            NUnitRegistry registry = new NUnitRegistry(runtimeVersion, new NUnitInfo[0], versions);
            WarningMessage warningMessage = new WarningMessage();
            NUnitSelector selector = new NUnitSelector(warningMessage.Handler,
                registry, minVersion, maxVersion, minVersion);

            string libDir = selector.GetLibDir(frameworkVersion);

            Assert.That(libDir, Is.Null);
            Assert.That(warningMessage.Text, Is.Not.Null);
            StringAssert.Contains(runtimeVersion, warningMessage.Text,
                "Expect message re: unknown runtime version.");
            StringAssert.Contains("www.testdriven.net/download", warningMessage.Text,
                "Should suggest installing TestDriven.Net.");
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
