using System;
using System.IO;

namespace NUnit.AddInRunner
{
    public class NUnitInfo
    {
        Version productVersion;
        string baseDir;

        public NUnitInfo(Version productVersion, string baseDir)
        {
            this.productVersion = productVersion;
            this.baseDir = baseDir;
        }

        public Version ProductVersion
        {
            get { return productVersion; }
        }

        public string BaseDir
        {
            get { return baseDir; }
        }
    }
}
