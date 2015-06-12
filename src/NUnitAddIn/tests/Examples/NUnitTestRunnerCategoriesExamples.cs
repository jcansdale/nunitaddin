namespace NUnit.AddInRunner.Examples
{
    namespace Categories
    {
        using NUnit.Framework;

        [TestFixture]
        public class CategoriesTests
        {
            [Test]
            public void CategoryNone()
            {
            }

            [Test, Category("A")]
            public void CategoryA()
            {
            }

            [Test, Category("B")]
            public void CategoryB()
            {
                Assert.Fail("CategoryB");
            }
        }

        [TestFixture, Category("C")]
        public class CategoryTestFixture
        {
            [Test]
            public void TestA()
            {
            }

            [Test]
            public void TestB()
            {
                Assert.Fail("TestB");
            }
        }
    }
}
