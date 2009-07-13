using System;
using System.IO;

namespace NUnit.AddInRunner
{
    public class NUnitInfo
    {
        Version productVersion;
        string runtimeVersion;
        string baseDir;
        string libDir;

        public NUnitInfo(Version productVersion, string runtimeVersion, string baseDir)
        {
            this.productVersion = productVersion;
            this.runtimeVersion = runtimeVersion;
            this.baseDir = baseDir;
            this.libDir = Path.Combine(baseDir, "lib");
        }

        public Version ProductVersion
        {
            get { return productVersion; }
        }

        public string RuntimeVersion
        {
            get { return runtimeVersion; }
        }

        public string BaseDir
        {
            get { return baseDir; }
        }

        public string LibDir
        {
            get { return libDir; }
        }
    }
}
