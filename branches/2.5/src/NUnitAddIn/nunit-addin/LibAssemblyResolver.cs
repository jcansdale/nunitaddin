namespace NUnit.AddInRunner
{
    using System;
    using System.IO;
    using System.Reflection;
    using TestDriven.Framework;

    public class LibAssemblyResolver : IDisposable
    {
        readonly string dir;
        ITestListener testListener;

        public LibAssemblyResolver(ITestListener testListener, Assembly targetAssembly)
        {
            this.testListener = testListener;
            WarningMessageHandler warningMessageHandler = new WarningMessageHandler(warningMessage);

            Assembly frameworkAssembly = FrameworkUtilities.FindFrameworkAssembly(targetAssembly);
            if(frameworkAssembly == null)
            {
                warningMessageHandler("Couldn't find reference to 'nunit.framework' on " + targetAssembly.CodeBase);
                return;
            }

            NUnitRegistry nunitRegistry = NUnitRegistry.Load(@"Software\nunit.org\Nunit");
            Version minVersion = new Version("2.5.0.0");
            Version maxVersion = new Version("2.5.65536.65536");
            Version rtmVersion = new Version("2.5.0.9122");
            NUnitSelector selector = new NUnitSelector(warningMessageHandler,
                nunitRegistry, minVersion, maxVersion, rtmVersion);
            Version frameworkVersion = frameworkAssembly.GetName().Version;
            string runtimeVersion = frameworkAssembly.ImageRuntimeVersion;
            this.dir = selector.GetLibDir(frameworkVersion);
            AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(AssemblyResolve);
        }

        void warningMessage(string text)
        {
            testListener.WriteLine(text, Category.Warning);
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
