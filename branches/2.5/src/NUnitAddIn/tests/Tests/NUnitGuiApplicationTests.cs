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
            NUnitSelector selector = createSelector(baseDir, new Version("2.5.0.0"), "v2.0.50727");
            NUnitGuiApplication application = new NUnitGuiApplication();
            string assemblyFile = new Uri(GetType().Assembly.CodeBase).LocalPath;

            string exeFile = application.FindApplication(selector, assemblyFile, false);

            Assert.That(exeFile, Is.EqualTo(expectedFile));
        }

        [Test]
        public void FindApplicationPath_32Bit()
        {
            const string baseDir = @"c:\base\dir";
            string expectedFile = Path.Combine(baseDir, "nunit-x86.exe");
            NUnitSelector selector = createSelector(baseDir, new Version("2.5.0.0"), "v2.0.50727");
            NUnitGuiApplication application = new NUnitGuiApplication();
            string assemblyFile = new Uri(GetType().Assembly.CodeBase).LocalPath;

            string exeFile = application.FindApplication(selector, assemblyFile, true);

            Assert.That(exeFile, Is.EqualTo(expectedFile));
        }

        static NUnitSelector createSelector(string baseDir, Version productVersion, string runtimeVersion)
        {
            NUnitInfo info = new NUnitInfo(productVersion, runtimeVersion, baseDir);
            NUnitInfo[] versions = new NUnitInfo[] { info };
            NUnitRegistry registry = new NUnitRegistry(runtimeVersion, new NUnitInfo[0], versions, new NUnitInfo[0]);
            WarningMessage warningMessage = new WarningMessage();
            NUnitSelector selector = new NUnitSelector(warningMessage.Handler, registry, productVersion, productVersion, productVersion);
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
