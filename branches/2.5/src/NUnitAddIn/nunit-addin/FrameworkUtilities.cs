namespace NUnit.AddInRunner
{
    using System;
    using System.IO;
    using System.Reflection;
    using System.Globalization;
    using Microsoft.Win32;

    public class FrameworkUtilities
    {
        public static string GetLibDir(string binDir, string runtimeVersion)
        {
            string versionDir;
            switch (runtimeVersion)
            {
                case "v1.1.4322":
                    versionDir = "net-1.1";
                    break;
                case "v2.0.50727":
                    versionDir = "net-2.0";
                    break;
                // There might be a .NET 4.0 version in future.
                default:
                    versionDir = "net-2.0";
                    break;
            }

            string libDir = Path.Combine(versionDir, "lib");
            libDir = Path.Combine(binDir, libDir);
            return libDir;
        }

        public static Assembly FindFrameworkAssembly(Assembly targetAssembly)
        {
            foreach (AssemblyName assemblyName in targetAssembly.GetReferencedAssemblies())
            {
                if (assemblyName.Name.ToLower(CultureInfo.InvariantCulture) == "nunit.framework")
                {
                    return Assembly.Load(assemblyName);
                }
            }

            return null;
        }

        public static NUnitInfo FindInstallDir(NUnitInfo[] versions,
            Version minVersion, Version maxVersion, string runtimeVersion)
        {
            NUnitInfo nunitVersion = null;
            foreach (NUnitInfo info in versions)
            {
                if (info.ProductVersion < minVersion || info.ProductVersion > maxVersion ||
                    info.RuntimeVersion != runtimeVersion)
                {
                    continue;
                }

                minVersion = info.ProductVersion;
                nunitVersion = info;
            }

            return nunitVersion;
        }
    }
}
