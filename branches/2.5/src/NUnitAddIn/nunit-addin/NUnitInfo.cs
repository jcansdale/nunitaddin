using System;

namespace NUnit.AddInRunner
{
    public class NUnitInfo
    {
        string installDir;
        Version productVersion;
        string runtimeVersion;
        string libDir;

        public NUnitInfo(string installDir, Version productVersion,
            string runtimeVersion, string libDir)
        {
            this.installDir = installDir;
            this.productVersion = productVersion;
            this.runtimeVersion = runtimeVersion;
            this.libDir = libDir;
        }

        public string InstallDir
        {
            get { return installDir; }
        }

        public Version ProductVersion
        {
            get { return productVersion; }
        }

        public string RuntimeVersion
        {
            get { return runtimeVersion; }
        }

        public string LibDir
        {
            get { return libDir; }
        }
    }
}
