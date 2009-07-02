using Microsoft.Win32;
using System;
using System.Collections;

namespace NUnit.AddInRunner
{
    public class NUnitRegistry
    {
        NUnitInfo[] versions;

        public static NUnitRegistry Load(string nunitKeyName)
        {
            ArrayList infoList = new ArrayList();
            using (RegistryKey nunitKey = Registry.CurrentUser.OpenSubKey(nunitKeyName))
            {
                if (nunitKey != null)
                {
                    foreach (string name in nunitKey.GetSubKeyNames())
                    {
                        using (RegistryKey versionKey = nunitKey.OpenSubKey(name))
                        {
                            if (versionKey == null)
                            {
                                continue;
                            }

                            string productVersion = (string) versionKey.GetValue("ProductVersion");
                            if (productVersion == null)
                            {
                                continue;
                            }

                            string installDir = (string) versionKey.GetValue("InstallDir");
                            if (installDir == null)
                            {
                                continue;
                            }

                            NUnitInfo info = new NUnitInfo(installDir, new Version(productVersion));
                            infoList.Add(info);
                        }
                    }
                }
            }

            return new NUnitRegistry((NUnitInfo[])infoList.ToArray(typeof(NUnitInfo)));
        }

        public NUnitRegistry(NUnitInfo[] versions)
        {
            this.versions = versions;
        }

        public NUnitInfo[] Versions
        {
            get { return versions; }
        }
    }
}
