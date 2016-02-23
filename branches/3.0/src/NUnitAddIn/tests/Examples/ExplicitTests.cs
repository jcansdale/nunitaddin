namespace NUnit.AddInRunner.Examples
{
    using System;
    using NUnit.Framework;

    namespace ExplicitNamespace
    {
        public class ExplicitTests
        {
            [Test, Explicit]
            public void ExplicitTest()
            {
            }
        }

        [Explicit]
        [TestFixture]
        public class ExplicitFixture
        {
            [Test]
            public void Test()
            {
            }
        }
    }
}
