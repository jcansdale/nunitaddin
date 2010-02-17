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

        public static string FrameworkAssemblyName = "nunit.framework";

        public static AssemblyName FindFrameworkAssembyName(string assemblyFile, AssemblyName[] assemblyNames)
        {
            AssemblyName assemblyName = FindFrameworkAssembyName(assemblyNames);
            if(assemblyName != null)
            {
                return assemblyName;
            }

            // TODO: Create unit tests for this.
            string dir = Path.GetDirectoryName(assemblyFile);
            string frameworkAssemblyFile = Path.Combine(dir, FrameworkAssemblyName + ".dll");
            if (File.Exists(frameworkAssemblyFile))
            {
                return AssemblyName.GetAssemblyName(frameworkAssemblyFile);
            }

            return null;
        }

        public static AssemblyName FindFrameworkAssembyName(AssemblyName[] assemblyNames)
        {
            foreach (AssemblyName assemblyName in assemblyNames)
            {
                if (assemblyName.Name.ToLower(CultureInfo.InvariantCulture) == FrameworkAssemblyName)
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