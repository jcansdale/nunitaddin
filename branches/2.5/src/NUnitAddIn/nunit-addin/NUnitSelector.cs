using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace NUnit.AddInRunner
{
    public class NUnitSelector
    {
        readonly WarningMessageHandler warningMessageHandler;
        readonly NUnitRegistry nunitRegistry;
        readonly Version minVersion;
        readonly Version maxVersion;
        readonly Version rtmVersion;

        public NUnitSelector(WarningMessageHandler warningMessageHandler,
            NUnitRegistry nunitRegistry, Version minVersion, Version maxVersion, Version rtmVersion)
        {
            this.warningMessageHandler = warningMessageHandler;
            this.nunitRegistry = nunitRegistry;
            this.minVersion = minVersion;
            this.maxVersion = maxVersion;
            this.rtmVersion = rtmVersion;
        }

        public NUnitInfo GetInfo(Version frameworkVersion)
        {
            NUnitInfo info = getDeveloperInfo(frameworkVersion, nunitRegistry.RuntimeVersion);
            if (info != null)
            {
                return info;
            }

            info = getInstalledInfo(frameworkVersion, nunitRegistry.RuntimeVersion);
            if (info != null)
            {
                return info;
            }

            info = getDefaultInfo(frameworkVersion, nunitRegistry.RuntimeVersion);
            if (info != null)
            {
                return info;
            }

            return null;
        }

        NUnitInfo getDeveloperInfo(Version frameworkVersion, string runtimeVersion)
        {
            NUnitInfo info = findInstallDir(
                nunitRegistry.DeveloperVersions, minVersion, maxVersion, runtimeVersion);
            if (info == null)
            {
                return null;
            }

            return info;
        }

        NUnitInfo getInstalledInfo(Version frameworkVersion, string runtimeVersion)
        {
            NUnitInfo info = findInstallDir(
                nunitRegistry.InstalledVersions, minVersion, maxVersion, runtimeVersion);
            if (info == null)
            {
                return null;
            }

            if (info.ProductVersion < rtmVersion)
            {
                warningMessageHandler(string.Format(
@"Please install NUnit {0} RTM or greater:
http://nunit.com/index.php?p=download
", rtmVersion));
                return null;
            }

            if (info.ProductVersion < frameworkVersion)
            {
                warningMessageHandler(string.Format(
@"Please install NUnit {0} or greater:
http://nunit.com/index.php?p=download
", frameworkVersion));

                return null;
            }

            return info;
        }

        NUnitInfo getDefaultInfo(Version frameworkVersion, string runtimeVersion)
        {
            NUnitInfo info = findInstallDir(nunitRegistry.DefaultVersions,
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

            return info;
        }

        static NUnitInfo findInstallDir(NUnitInfo[] versions,
            Version minVersion, Version maxVersion, string runtimeVersion)
        {
            Version minRuntimeVersion = new Version(0, 0);
            Version maxRuntimeVersion = toVersion(runtimeVersion);
            NUnitInfo nunitVersion = null;
            foreach (NUnitInfo info in versions)
            {
                if (info.ProductVersion < minVersion || info.ProductVersion > maxVersion ||
                    toVersion(info.RuntimeVersion) > maxRuntimeVersion ||
                    toVersion(info.RuntimeVersion) < minRuntimeVersion)
                {
                    continue;
                }

                minVersion = info.ProductVersion;
                minRuntimeVersion = toVersion(info.RuntimeVersion);
                nunitVersion = info;
            }

            return nunitVersion;
        }

        static Version toVersion(string runtimeVersion)
        {
            return new Version(runtimeVersion.Substring(1));
        }
    }
}
