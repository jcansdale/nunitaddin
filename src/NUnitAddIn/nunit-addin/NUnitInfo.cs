using System;

namespace NUnit.AddInRunner
{
    public class NUnitInfo
    {
        private string installDir;
        private Version productVersion;

        public NUnitInfo(string installDir, Version productVersion)
        {
            this.installDir = installDir;
            this.productVersion = productVersion;
        }

        public string InstallDir
        {
            get { return installDir; }
        }

        public Version ProductVersion
        {
            get { return productVersion; }
        }
    }
}
