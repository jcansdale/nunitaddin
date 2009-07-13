using System.Reflection;

namespace NUnit.AddInRunner
{
    using System;
    using System.IO;
    using System.Collections;
    using Microsoft.Win32;

    public class NUnitRegistry
    {
        readonly NUnitInfo[] defaultVersions;
        readonly NUnitInfo[] installedVersions;
        readonly NUnitInfo[] developerVersions;
        readonly string runtimeVersion;

        public static NUnitRegistry Load(string nunitKeyName, string targetDir, string runtimeVersion)
        {
            NUnitInfo[] developerVersions = LoadDeveloperVersions(runtimeVersion,
                AppDomain.CurrentDomain.BaseDirectory, Constants.ConfigFileName);
            NUnitInfo[] defaultVersions = LoadDefaultVersions(runtimeVersion);
            NUnitInfo[] installedVersions = LoadInstalledVersions(nunitKeyName);
            return new NUnitRegistry(runtimeVersion, defaultVersions, installedVersions, developerVersions);
        }

        public static NUnitInfo[] LoadDeveloperVersions(string runtimeVersion, string targetDir, string configFileName)
        {
            while(targetDir != null)
            {
                string file = Path.Combine(targetDir, configFileName);
                if (File.Exists(file))
                {
                    return loadConfigVersions(runtimeVersion, file);
                }

                targetDir = Path.GetDirectoryName(targetDir);
            }

            return new NUnitInfo[0];
        }

        public static NUnitInfo[] LoadDefaultVersions(string runtimeVersion)
        {
            string dir = getDefaultBinDir();
            string configFile = Path.Combine(dir, Constants.ConfigFileName);
            return loadConfigVersions(runtimeVersion, configFile);
        }

        static NUnitInfo[] loadConfigVersions(string runtimeVersion, string configFile)
        {
            NUnitConfig config = NUnitConfig.Load(configFile);

            string dir = Path.GetDirectoryName(configFile);
            ArrayList infoList = new ArrayList();
            foreach (NUnitConfig.Info lib in config.Infos)
            {
                NUnitInfo info = getConfigVersion(runtimeVersion, dir, lib);

                if (info != null)
                {
                    infoList.Add(info);
                }
            }

            return (NUnitInfo[])infoList.ToArray(typeof(NUnitInfo));
        }

        static NUnitInfo getConfigVersion(string runtimeVersion, string dir, NUnitConfig.Info info)
        {
            if (toVersion(runtimeVersion) < toVersion(info.RuntimeVersion))
            {
                return null;
            }

            string baseDir = Path.Combine(dir, info.BaseDir);
            baseDir = Path.GetFullPath(baseDir);
            Version productVersion = info.ProductVersion;
            if(productVersion == null)
            {
                string coreFile = Path.Combine(baseDir, @"lib\nunit.core.dll");
                AssemblyName assemblyName = AssemblyName.GetAssemblyName(coreFile);
                productVersion = assemblyName.Version;
            }

            return new NUnitInfo(productVersion, info.RuntimeVersion, baseDir);
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

        public static NUnitInfo[] LoadInstalledVersions(string nunitKeyName)
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
                        string baseDir11 = Path.Combine(binDir, "net-1.1");
                        if (Directory.Exists(baseDir11))
                        {
                            NUnitInfo info = new NUnitInfo(version, runtimeVersion11, baseDir11);
                            infoList.Add(info);
                        }

                        const string runtimeVersion20 = "v2.0.50727";
                        string baseDir20 = Path.Combine(binDir, "net-2.0");
                        if (Directory.Exists(baseDir20))
                        {
                            NUnitInfo info = new NUnitInfo(version, runtimeVersion20, baseDir20);
                            infoList.Add(info);
                        }
                    }
                }

                return (NUnitInfo[])infoList.ToArray(typeof(NUnitInfo));
            }
        }

        public NUnitRegistry(string runtimeVersion, NUnitInfo[] defaultVersions,
            NUnitInfo[] installedVersions, NUnitInfo[] developerVersions)
        {
            this.runtimeVersion = runtimeVersion;
            this.defaultVersions = defaultVersions;
            this.installedVersions = installedVersions;
            this.developerVersions = developerVersions;
        }

        public NUnitInfo[] DefaultVersions
        {
            get { return defaultVersions; }
        }

        public NUnitInfo[] InstalledVersions
        {
            get { return installedVersions; }
        }

        public NUnitInfo[] DeveloperVersions
        {
            get { return developerVersions; }
        }

        public string RuntimeVersion
        {
            get { return runtimeVersion; }
        }
    }
}
