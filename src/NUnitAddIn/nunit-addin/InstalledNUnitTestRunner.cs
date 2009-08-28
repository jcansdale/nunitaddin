namespace NUnit.AddInRunner
{
    public class InstalledNUnitTestRunner : NUnitTestRunnerBase
    {
        public InstalledNUnitTestRunner()
            : base(Constants.NUnitRegistryRoot)
        {
        }
    }
}
