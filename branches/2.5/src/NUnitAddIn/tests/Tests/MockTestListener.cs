namespace NUnit.AddInRunner.Tests
{
    using TestDriven.Framework;
    using System.Collections;

    class MockTestListener : ITestListener
    {
        public int TestFinishedCount;
        public int TestResultsUrlCount;
        public int WriteLineCount;
        public int SuccessCount;
        public int FailureCount;
        public int IgnoredCount;
        ArrayList testResults = new ArrayList();

        public TestResult[] TestResults
        {
            get { return (TestResult[])testResults.ToArray(typeof(TestResult)); }
        }

        public void TestFinished(TestResult summary)
        {
            this.testResults.Add(summary);
            this.TestFinishedCount++;
            switch (summary.State)
            {
                case TestState.Passed:
                    this.SuccessCount++;
                    break;
                case TestState.Failed:
                    this.FailureCount++;
                    break;
                case TestState.Ignored:
                    this.IgnoredCount++;
                    break;
            }
        }

        public void TestResultsUrl(string resultsUrl)
        {
            this.TestResultsUrlCount++;
        }

        public void WriteLine(string text, Category category)
        {
            this.WriteLineCount++;
        }
    }
}