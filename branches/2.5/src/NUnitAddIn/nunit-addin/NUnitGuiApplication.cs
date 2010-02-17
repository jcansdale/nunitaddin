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
            if(info == null)
            {
                return null;
            }

            string fileName = is32Bit ? "nunit-x86.exe" : "nunit.exe";
            return Path.Combine(info.BaseDir, fileName);
        }

        static Version getFrameworkVersion(string assemblyFile)
        {
            AssemblyName[] assemblyNames = AssemblyReflector.GetReferencedAssemblies(assemblyFile);
            AssemblyName frameworkAssembly = FrameworkUtilities.FindFrameworkAssembyName(assemblyFile, assemblyNames);
            if(frameworkAssembly != null)
            {
                return frameworkAssembly.Version;
            }

            warning("Couldn't find framework assembly for:");
            warning(assemblyFile);
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
                Type type = typeof(AssemblyReflector);
                using (RemoteObject remoteObject = new RemoteObject(type))
                {
                    AssemblyReflector reflector = (AssemblyReflector)remoteObject.CreateInstance();
                    return reflector.getReferencedAssemblies(assemblyFile);
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

        class RemoteObject : IDisposable
        {
            readonly Type type;
            AppDomain domain;

            public RemoteObject(Type type)
            {
                this.type = type;
                domain = AppDomain.CreateDomain("GetReferencedAssemblies");
                AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(CurrentDomain_AssemblyResolve);
            }

            public void Dispose()
            {
                AppDomain.CurrentDomain.AssemblyResolve -= new ResolveEventHandler(CurrentDomain_AssemblyResolve);
                AppDomain.Unload(domain);
            }

            public object CreateInstance()
            {
                return domain.CreateInstanceFromAndUnwrap(type.Assembly.CodeBase, type.FullName);
            }

            Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
            {
                Assembly assembly = type.Assembly;
                if(args.Name == assembly.FullName)
                {
                    return assembly;
                }

                return null;
            }
        }
    }
}
