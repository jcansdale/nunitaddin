namespace NUnit.AddInRunner.Tests
{
    using System;
    using System.IO;
    using Microsoft.Win32;
    using NUnit.Framework;

    public class NUnitRegistryTests
    {
        public void Load()
        {
            Version version = new Version("2.5.666.1");
            const string nunitKeyName = @"Software\nunit.org\Nunit_Test";
            string subkey = Path.Combine(nunitKeyName, "__TEST__");

            const string expectedDir = @"x:\testdir";
            using (RegistryKey key = Registry.CurrentUser.CreateSubKey(subkey))
            {
                key.SetValue("InstallDir", expectedDir);
                key.SetValue("ProductVersion", version);
            }

            try
            {
                NUnitRegistry nunitRegistry = NUnitRegistry.Load(nunitKeyName);
                NUnitInfo info = nunitRegistry.Versions[0];
                Assert.That(info.InstallDir, Is.EqualTo(expectedDir));
                Assert.That(info.ProductVersion, Is.EqualTo(version));
            }
            finally
            {
                Registry.CurrentUser.DeleteSubKey(subkey);
                Registry.CurrentUser.DeleteSubKey(nunitKeyName);
            }
        }
    }
}
