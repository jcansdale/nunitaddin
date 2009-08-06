using System.IO;

namespace NUnit.AddInRunner
{
    using System;
    using System.Reflection;
    using System.Globalization;

    public class FrameworkUtilities
    {
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
    }
}