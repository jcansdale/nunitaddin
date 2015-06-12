namespace NUnit.AddInRunner
{
    using System;
    using System.IO;
    using System.Reflection;
    using System.Globalization;

    public class LibAssemblyResolver : IDisposable
    {
        readonly string dir;

        public LibAssemblyResolver(string libDir)
        {
            this.dir = libDir;
            AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(AssemblyResolve);
        }

        public void Dispose()
        {
            AppDomain.CurrentDomain.AssemblyResolve -= new ResolveEventHandler(AssemblyResolve);
        }

        Assembly AssemblyResolve(object sender, ResolveEventArgs args)
        {
            string[] split = args.Name.Split(new char[] { ',' });
            string assemblyName = split[0];

            // Only match NUnit assemblies.
            if(!assemblyName.ToLower(CultureInfo.InvariantCulture).StartsWith("nunit."))
            {
                return null;
            }

            string assemblyFile = Path.Combine(dir, assemblyName + ".dll");
            if (!File.Exists(assemblyFile))
            {
                return null;
            }

            return Assembly.LoadFrom(assemblyFile);
        }
    }
}
