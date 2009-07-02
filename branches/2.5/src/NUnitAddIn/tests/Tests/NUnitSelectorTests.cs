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
            NUnitRegistry registry = new NUnitRegistry(new NUnitInfo[0]);
            NUnitSelector selector = new NUnitSelector(registry,
                frameworkVersion, frameworkVersion, frameworkVersion);

            string libDir = selector.GetLibDir(frameworkVersion, "v2.0.50727");

            Assert.That(libDir, Is.Not.Null);
            Assert.That(Directory.Exists(libDir));
        }

        [Test]
        public void GetLibDir_Installed()
        {
            Version minVersion = new Version("2.5.0.0");
            Version maxVersion = new Version("2.5.65536.65536");
            const string installDir = @"c:\install\dir";
            string expectedLibDir = Path.Combine(installDir, @"bin\net-2.0\lib");
            Version frameworkVersion = new Version("2.5.5.0");
            NUnitInfo[] versions = new NUnitInfo[] { new NUnitInfo(installDir, frameworkVersion) };
            NUnitRegistry registry = new NUnitRegistry(versions);
            NUnitSelector selector = new NUnitSelector(registry, minVersion, maxVersion, minVersion);

            string libDir = selector.GetLibDir(frameworkVersion, "v2.0.50727");

            Assert.That(libDir, Is.Not.Null);
            Assert.That(libDir, Is.EqualTo(expectedLibDir));
        }

        [Test]
        public void GetLibDir_Incompatible()
        {
            Version minVersion = new Version("2.5.0.0");
            Version maxVersion = new Version("2.5.65536.65536");
            Version rtmVersion = new Version("2.5.0.9122");
            const string installDir = @"c:\install\dir";
            string expectedLibDir = Path.Combine(installDir, @"bin\net-2.0\lib");
            Version frameworkVersion = new Version("2.5.0.9015"); // Beta2
            NUnitInfo[] versions = new NUnitInfo[] { new NUnitInfo(installDir, frameworkVersion) };
            NUnitRegistry registry = new NUnitRegistry(versions);
            NUnitSelector selector = new NUnitSelector(registry, minVersion, maxVersion, rtmVersion);

            string libDir = selector.GetLibDir(frameworkVersion, "v2.0.50727");

            Assert.That(libDir, Is.Not.EqualTo(expectedLibDir));
        }

        [Test]
        public void GetLibDir_LaterFramework()
        {
            Version minVersion = new Version("2.5.0.0");
            Version maxVersion = new Version("2.5.65536.65536");
            Version rtmVersion = new Version("2.5.0.9122");
            Version frameworkVersion = new Version("2.5.10.10000");
            const string installDir = @"c:\install\dir";
            string expectedLibDir = Path.Combine(installDir, @"bin\net-2.0\lib");
            NUnitInfo[] versions = new NUnitInfo[] { new NUnitInfo(installDir, rtmVersion) };
            NUnitRegistry registry = new NUnitRegistry(versions);
            NUnitSelector selector = new NUnitSelector(registry, minVersion, maxVersion, rtmVersion);

            string libDir = selector.GetLibDir(frameworkVersion, "v2.0.50727");

            Assert.That(libDir, Is.Not.EqualTo(expectedLibDir));
        }
    }
}
