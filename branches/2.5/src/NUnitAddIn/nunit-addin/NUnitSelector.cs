using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace NUnit.AddInRunner
{
    public class NUnitSelector
    {
        WarningMessageHandler warningMessageHandler;
        NUnitRegistry nunitRegistry;
        Version minVersion;
        Version maxVersion;
        Version rtmVersion;

        public NUnitSelector(WarningMessageHandler warningMessageHandler,
            NUnitRegistry nunitRegistry, Version minVersion, Version maxVersion, Version rtmVersion)
        {
            this.warningMessageHandler = warningMessageHandler;
            this.nunitRegistry = nunitRegistry;
            this.minVersion = minVersion;
            this.maxVersion = maxVersion;
            this.rtmVersion = rtmVersion;
        }

        public string GetLibDir(Version frameworkVersion)
        {
            string libDir = getInstallLibDir(frameworkVersion, nunitRegistry.RuntimeVersion);
            if(libDir == null)
            {
                libDir = getDefaultLibDir(frameworkVersion, nunitRegistry.RuntimeVersion);
            }

            return libDir;
        }

        string getInstallLibDir(Version frameworkVersion, string runtimeVersion)
        {
            NUnitInfo info = FrameworkUtilities.FindInstallDir(
                nunitRegistry.Versions, minVersion, maxVersion, runtimeVersion);
            if (info == null)
            {
                return null;
            }

            if(info.ProductVersion < rtmVersion)
            {
                warningMessageHandler(string.Format(
@"Please install NUnit {0} RTM or greater:
http://nunit.com/index.php?p=download
", rtmVersion));
                return null;
            }

            if(info.ProductVersion < frameworkVersion)
            {
                warningMessageHandler(string.Format(
@"Please install NUnit {0} or greater:
http://nunit.com/index.php?p=download
", frameworkVersion));

                return null;
            }

            return info.LibDir;
        }

        string getDefaultLibDir(Version frameworkVersion, string runtimeVersion)
        {
            NUnitInfo info = FrameworkUtilities.FindInstallDir(nunitRegistry.DefaultVersions,
                minVersion, maxVersion, runtimeVersion);

            if(info == null)
            {
                warningMessageHandler(string.Format(
@"Couldn't find NUnit test runner for .NET {0}.
Please install the latest version of TestDriven.Net:
http://www.testdriven.net/download.aspx

or let support@testdriven know you're seeing this message.
", runtimeVersion));
                return null;
            }

            if(info.ProductVersion < frameworkVersion)
            {
                warningMessageHandler(string.Format(
@"The packaged version of NUnit is older than {0}.
Please install the latest version of TestDriven.Net:
http://www.testdriven.net/download.aspx

or install NUnit {0} or later:
http://nunit.com/index.php?p=download
", frameworkVersion));
            }

            return info.LibDir;
        }
    }
}
