namespace NUnit.AddInRunner
{
    using System;
    using System.IO;
    using System.Reflection;

    public class LibAssemblyResolver : IDisposable
    {
        readonly string dir;

        public LibAssemblyResolver(Assembly targetAssembly)
        {
            Assembly frameworkAssembly = FrameworkUtilities.FindFrameworkAssembly(targetAssembly);
            if(frameworkAssembly == null)
            {
                throw new Exception("Couldn't find reference to 'nunit.framework' on " + targetAssembly.CodeBase);
            }

            NUnitRegistry nunitRegistry = NUnitRegistry.Load(@"Software\nunit.org\Nunit");
            Version minVersion = new Version("2.5.0.0");
            Version maxVersion = new Version("2.5.65536.65536");
            Version rtmVersion = new Version("2.5.0.9122");
            NUnitSelector selector = new NUnitSelector(nunitRegistry,
                minVersion, maxVersion, rtmVersion);
            Version frameworkVersion = frameworkAssembly.GetName().Version;
            string runtimeVersion = frameworkAssembly.ImageRuntimeVersion;
            this.dir = selector.GetLibDir(frameworkVersion, runtimeVersion);
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
            string assemblyFile = Path.Combine(dir, assemblyName + ".dll");
            if (!File.Exists(assemblyFile))
            {
                return null;
            }

            return Assembly.LoadFrom(assemblyFile);
        }
    }
}
