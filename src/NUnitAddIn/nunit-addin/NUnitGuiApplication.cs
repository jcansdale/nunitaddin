namespace NUnit.AddInRunner
{
    using System;
    using System.IO;
    using TestDriven.Framework.Applications;

    public class NUnitGuiApplication : IGuiApplication
    {
        public string FindApplication(string assemblyFile, bool is32Bit)
        {
            string targetDir = Path.GetDirectoryName(assemblyFile);

            const string runtimeVersion = "v2.0.50727";
            NUnitRegistry registry = NUnitRegistry.Load(Constants.NUnitRegistryRoot,
                targetDir, runtimeVersion);
            NUnitSelector selector = new NUnitSelector(new WarningMessageHandler(warning), registry,
                Constants.MinVersion, Constants.MaxVersion, Constants.RtmVersion);

            return FindApplication(selector, assemblyFile, is32Bit);
        }

        public string FindApplication(NUnitSelector selector, string assemblyFile, bool is32Bit)
        {
            Version frameworkVersion = new Version("2.5.0.0"); // Any NUnit 2.5 version will do.
            NUnitInfo info = selector.GetInfo(frameworkVersion);
            string fileName = is32Bit ? "nunit-x86.exe" : "nunit.exe";
            return Path.Combine(info.BaseDir, fileName);
        }

        static void warning(string text)
        {
            Console.WriteLine(text);
        }
    }
}
