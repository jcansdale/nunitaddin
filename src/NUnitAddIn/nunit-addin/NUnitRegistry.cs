using System.Reflection;

namespace NUnit.AddInRunner
{
    using System;
    using System.IO;
    using System.Collections;
    using Microsoft.Win32;
    using System.Runtime.InteropServices;

    public class NUnitRegistry
    {
        readonly NUnitInfo[] defaultVersions;
        readonly NUnitInfo[] versions;
        readonly string runtimeVersion;

        public static NUnitRegistry Load(string nunitKeyName)
        {
            return Load(nunitKeyName, RuntimeEnvironment.GetSystemVersion());
        }

        public static NUnitRegistry Load(string nunitKeyName, string runtimeVersion)
        {
            NUnitInfo[] defaultVersions = getDefaultVersions(runtimeVersion);
            NUnitInfo[] installedVersions = getInstalledVersions(nunitKeyName);
            return new NUnitRegistry(runtimeVersion, defaultVersions, installedVersions);
        }

        static NUnitInfo[] getDefaultVersions(string runtimeVersion)
        {
            ArrayList infoList = new ArrayList();

            NUnitInfo net11Version = getDefaultVersion(runtimeVersion, "v1.1.4322");
            if(net11Version != null) infoList.Add(net11Version);

            NUnitInfo net20Version = getDefaultVersion(runtimeVersion, "v2.0.50727");
            if (net20Version != null) infoList.Add(net20Version);

            return (NUnitInfo[])infoList.ToArray(typeof(NUnitInfo));
        }

        static NUnitInfo getDefaultVersion(string runtimeVersion, string imageRuntimeVersion)
        {
            if(toVersion(runtimeVersion) < toVersion(imageRuntimeVersion))
            {
                return null;
            }

            string binDir = getDefaultBinDir();
            string libDir = FrameworkUtilities.GetLibDir(binDir, imageRuntimeVersion);
            string coreFile = Path.Combine(libDir, "nunit.core.dll");
            AssemblyName assemblyName = AssemblyName.GetAssemblyName(coreFile);
            return new NUnitInfo(null, assemblyName.Version, imageRuntimeVersion, libDir);
        }

        static Version toVersion(string runtimeVersion)
        {
            return new Version(runtimeVersion.Substring(1));
        }

        static string getDefaultBinDir()
        {
            string localPath = new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath;
            return Path.GetDirectoryName(localPath);
        }

        static NUnitInfo[] getInstalledVersions(string nunitKeyName)
        {
            using (RegistryKey nunitKey = Registry.CurrentUser.OpenSubKey(nunitKeyName))
            {
                if (nunitKey == null)
                {
                    return new NUnitInfo[0];
                }

                ArrayList infoList = new ArrayList();
                foreach (string name in nunitKey.GetSubKeyNames())
                {
                    using (RegistryKey versionKey = nunitKey.OpenSubKey(name))
                    {
                        if (versionKey == null)
                        {
                            continue;
                        }

                        string productVersion = (string)versionKey.GetValue("ProductVersion");
                        if (productVersion == null)
                        {
                            continue;
                        }

                        string installDir = (string)versionKey.GetValue("InstallDir");
                        if (installDir == null)
                        {
                            continue;
                        }

                        string binDir = Path.Combine(installDir, "bin");
                        Version version = new Version(productVersion);

                        const string runtimeVersion11 = "v1.1.4322";
                        string libDir11 = FrameworkUtilities.GetLibDir(binDir, runtimeVersion11);
                        if (Directory.Exists(libDir11))
                        {
                            NUnitInfo info = new NUnitInfo(installDir, version,
                                runtimeVersion11, libDir11);
                            infoList.Add(info);
                        }

                        const string runtimeVersion20 = "v2.0.50727";
                        string libDir20 = FrameworkUtilities.GetLibDir(binDir, runtimeVersion20);
                        if (Directory.Exists(libDir20))
                        {
                            NUnitInfo info = new NUnitInfo(installDir, version,
                                runtimeVersion20, libDir20);
                            infoList.Add(info);
                        }
                    }
                }

                return (NUnitInfo[])infoList.ToArray(typeof(NUnitInfo));
            }
        }

        public NUnitRegistry(NUnitInfo[] versions) :
            this(RuntimeEnvironment.GetSystemVersion(), new NUnitInfo[0], versions)
        {
        }

        public NUnitRegistry(string runtimeVersion, NUnitInfo[] defaultVersions, NUnitInfo[] versions)
        {
            this.runtimeVersion = runtimeVersion;
            this.defaultVersions = defaultVersions;
            this.versions = versions;
        }

        public NUnitInfo[] DefaultVersions
        {
            get { return defaultVersions; }
        }

        public NUnitInfo[] Versions
        {
            get { return versions; }
        }

        public string RuntimeVersion
        {
            get { return runtimeVersion; }
        }
    }
}
