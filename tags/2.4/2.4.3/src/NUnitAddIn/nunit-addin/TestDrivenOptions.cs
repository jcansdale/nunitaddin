namespace TestDriven.Framework.Options
{
    using System;
    using Microsoft.Win32;

    internal class TestDrivenOptions
    {
        internal static string[] IncludeCategories
        {
            get
            {
                return (string[])AppDomain.CurrentDomain.GetData("IncludeCategories");
            }
        }

        internal static string[] ExcludeCategories
        {
            get
            {
                return (string[])AppDomain.CurrentDomain.GetData("ExcludeCategories");
            }
        }
    }
}
