using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace NUnit.AddInRunner
{
    public class NUnitSelector
    {
        NUnitRegistry nunitRegistry;
        Version minVersion;
        Version maxVersion;
        Version rtmVersion;

        public NUnitSelector(NUnitRegistry nunitRegistry, Version minVersion, Version maxVersion, Version rtmVersion)
        {
            this.nunitRegistry = nunitRegistry;
            this.minVersion = minVersion;
            this.maxVersion = maxVersion;
            this.rtmVersion = rtmVersion;
        }

        public string GetLibDir(Version frameworkVersion, string runtimeVersion)
        {
            // TODO: Warn when .NET 1.1 feature not installed.

            string binDir = getBinDir(frameworkVersion);
            return FrameworkUtilities.GetLibDir(binDir, runtimeVersion);
        }

        string getBinDir(Version frameworkVersion)
        {
            NUnitInfo info = FrameworkUtilities.FindInstallDir(nunitRegistry, minVersion, maxVersion);
            if (info != null)
            {
                if(info.ProductVersion < rtmVersion)
                {
                    // TODO: Use warning API.
                    Trace.WriteLine("Please install NUnit " + rtmVersion + " RTM or greater:");
                    Trace.WriteLine("http://nunit.com/index.php?p=download");
                    Trace.WriteLine("");
                    return getDefaultBinDir();
                }

                if(info.ProductVersion < frameworkVersion)
                {
                    // TODO: Use warning API.
                    Trace.WriteLine("Please install NUnit " + frameworkVersion + " or greater:");
                    Trace.WriteLine("http://nunit.com/index.php?p=download");
                    Trace.WriteLine("");
                    return getDefaultBinDir();
                }

                return Path.Combine(info.InstallDir, "bin");
            }

            return getDefaultBinDir();
        }

        static string getDefaultBinDir()
        {
            string localPath = new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath;
            return Path.GetDirectoryName(localPath);
        }
    }
}
