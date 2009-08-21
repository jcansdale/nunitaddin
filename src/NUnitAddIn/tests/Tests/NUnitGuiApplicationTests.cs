using System.Reflection;

namespace NUnit.AddInRunner.Tests
{
    using System;
    using System.IO;
    using NUnit.Framework;

    public class NUnitGuiApplicationTests
    {
        [Test]
        public void FindApplicationPath()
        {
            const string baseDir = @"c:\base\dir";
            string expectedFile = Path.Combine(baseDir, "nunit.exe");
            Version frameworkVersion = typeof (TestAttribute).Assembly.GetName().Version;
            NUnitSelector selector = createSelector(baseDir, frameworkVersion, "v2.0.50727");
            NUnitGuiApplication application = new NUnitGuiApplication();
            string assemblyFile = new Uri(GetType().Assembly.CodeBase).LocalPath;

            string exeFile = application.FindApplication(selector, assemblyFile, false);

            Assert.That(exeFile, Is.EqualTo(expectedFile));
        }

        [Test]
        public void FindApplicationPath_32Bit()
        {
            const string baseDir = @"c:\base\dir";
            Version frameworkVersion = typeof(TestAttribute).Assembly.GetName().Version;
            NUnitSelector selector = createSelector(baseDir, frameworkVersion, "v2.0.50727");
            NUnitGuiApplication application = new NUnitGuiApplication();
            string assemblyFile = new Uri(GetType().Assembly.CodeBase).LocalPath;

            string exeFile = application.FindApplication(selector, assemblyFile, true);

            string expectedFile = Path.Combine(baseDir, "nunit-x86.exe");
            Assert.That(exeFile, Is.EqualTo(expectedFile));
        }

        [Test]
        public void FindApplicationPath_NoFrameworkReference()
        {
            const string baseDir = @"c:\base\dir";
            string expectedFile = Path.Combine(baseDir, "nunit.exe");
            Version frameworkVersion = typeof(TestAttribute).Assembly.GetName().Version;
            NUnitSelector selector = createSelector(baseDir, frameworkVersion, "v2.0.50727");
            NUnitGuiApplication application = new NUnitGuiApplication();
            Assembly assembly = typeof (Uri).Assembly;
            string assemblyFile = new Uri(assembly.CodeBase).LocalPath;

            string exeFile = application.FindApplication(selector, assemblyFile, false);

            Assert.That(exeFile, Is.EqualTo(expectedFile));
        }

        static NUnitSelector createSelector(string baseDir, Version productVersion, string runtimeVersion)
        {
            NUnitInfo info = new NUnitInfo(productVersion, runtimeVersion, baseDir);
            NUnitInfo[] versions = new NUnitInfo[] { info };
            NUnitRegistry registry = new NUnitRegistry(runtimeVersion, new NUnitInfo[0], versions, new NUnitInfo[0]);
            WarningMessage warningMessage = new WarningMessage();
            NUnitSelector selector = new NUnitSelector(warningMessage.Handler, registry,
                Constants.MinVersion, Constants.MaxVersion, Constants.RtmVersion);
            return selector;
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
