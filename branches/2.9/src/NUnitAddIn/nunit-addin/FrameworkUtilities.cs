using System.IO;

namespace NUnit.AddInRunner
{
    using System;
    using System.Reflection;
    using System.Globalization;

    public class FrameworkUtilities
    {
        public static string[] RequiredLibFiles = 
            new string[] { "nunit.core.interfaces.dll", "nunit.core.dll", "nunit.util.dll" };

        public static AssemblyName FindFrameworkAssembyName(AssemblyName[] assemblyNames)
        {
            foreach (AssemblyName assemblyName in assemblyNames)
            {
                if (assemblyName.Name.ToLower(CultureInfo.InvariantCulture) == "nunit.framework")
                {
                    return assemblyName;
                }
            }

            return null;
        }

        public static bool IsInstalled(string baseDir)
        {
            string libDir = Path.Combine(baseDir, "lib");
            foreach (string fileName in RequiredLibFiles)
            {
                string requiredFile = Path.Combine(libDir, fileName);
                if (!File.Exists(requiredFile))
                {
                    return false;
                }
            }

            return true;
        }
    }
}