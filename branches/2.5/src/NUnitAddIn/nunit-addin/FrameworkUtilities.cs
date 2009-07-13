namespace NUnit.AddInRunner
{
    using System;
    using System.Reflection;
    using System.Globalization;

    public class FrameworkUtilities
    {
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
    }
}
