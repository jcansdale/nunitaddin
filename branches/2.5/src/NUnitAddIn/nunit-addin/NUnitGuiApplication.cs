using System.Reflection;

namespace NUnit.AddInRunner
{
    using System;
    using System.IO;
    using System.Diagnostics;
    using TestDriven.Framework.Applications;

    public class NUnitGuiApplication : IGuiApplication
    {
        public string FindApplication(string assemblyFile, bool is32Bit)
        {
            string targetDir = Path.GetDirectoryName(assemblyFile);

            const string runtimeVersion = "v2.0.50727";
            NUnitRegistry registry = NUnitRegistry.Load(Constants.NUnitRegistryRoot,
                targetDir, runtimeVersion);
            NUnitSelector selector = new NUnitSelector(new WarningMessageHandler(warning), registry,
                Constants.MinVersion, Constants.MaxVersion, Constants.RtmVersion);

            return FindApplication(selector, assemblyFile, is32Bit);
        }

        public string FindApplication(NUnitSelector selector, string assemblyFile, bool is32Bit)
        {
            Version frameworkVersion = getFrameworkVersion(assemblyFile);
            NUnitInfo info = selector.GetInfo(frameworkVersion);
            string fileName = is32Bit ? "nunit-x86.exe" : "nunit.exe";
            return Path.Combine(info.BaseDir, fileName);
        }

        static Version getFrameworkVersion(string assemblyFile)
        {
            AssemblyName[] assemblyNames = AssemblyReflector.GetReferencedAssemblies(assemblyFile);
            AssemblyName frameworkAssembly = FrameworkUtilities.FindFrameworkAssembyName(assemblyNames);
            if(frameworkAssembly != null)
            {
                return frameworkAssembly.Version;
            }

            warning("Couldn't find framework assembly for: " + assemblyFile);
            return new Version("2.5.0.0"); // Any NUnit 2.5 version will do.
        }

        static void warning(string text)
        {
            Trace.WriteLine(text);
        }

        class AssemblyReflector : MarshalByRefObject
        {
            public static AssemblyName[] GetReferencedAssemblies(string assemblyFile)
            {
                AppDomainSetup info = new AppDomainSetup();
                info.ApplicationBase = Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath);

                return getReferencedAssembliesInNewDomain(assemblyFile, info);
            }

            static AssemblyName[] getReferencedAssembliesInNewDomain(string assemblyFile, AppDomainSetup info)
            {
                AppDomain domain = AppDomain.CreateDomain("GetReferencedAssemblies", null, info);
                try
                {
                    Type type = typeof(AssemblyReflector);
                    AssemblyReflector reflector = (AssemblyReflector)domain.CreateInstanceAndUnwrap(type.Assembly.FullName, type.FullName);
                    return reflector.getReferencedAssemblies(assemblyFile);
                }
                finally
                {
                    AppDomain.Unload(domain);
                }
            }

            AssemblyName[] getReferencedAssemblies(string assemblyFile)
            {
                // NOTE: Mixed C++ can only be loaded with LoadFrom.
                Assembly assembly = reflectionOnlyLoadFromWhenAvailable(assemblyFile);
                return assembly.GetReferencedAssemblies();
            }

            static Assembly reflectionOnlyLoadFromWhenAvailable(string assemblyFile)
            {
                Assembly assembly;
                MethodInfo reflectionOnlyLoadFromMethod = typeof(Assembly).GetMethod("ReflectionOnlyLoadFrom", new Type[] { typeof(string) });
                if (reflectionOnlyLoadFromMethod != null)
                {
                    assembly = (Assembly)reflectionOnlyLoadFromMethod.Invoke(null, new object[] { assemblyFile });
                }
                else
                {
                    assembly = Assembly.LoadFrom(assemblyFile);
                }
                return assembly;
            }
        }
    }
}
