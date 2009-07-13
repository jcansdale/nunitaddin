using System;

namespace NUnit.AddInRunner
{
    public class Constants
    {
        public const string ConfigFileName = "nunit.config";
        public const string NUnitRegistryRoot = @"Software\nunit.org\Nunit";

        public static Version MinVersion = new Version("2.5.0.0");
        public static Version MaxVersion = new Version("2.5.65536.65536");
        public static Version RtmVersion = new Version("2.5.0.9122");
    }
}
