namespace NUnit.AddInRunner.Examples
{
    using NUnit.Framework;
    using System.Collections.Generic;
    using System.Collections;
    using System;

    public class ConsoleTests
    {
        [Test]
        public void HelloWorld()
        {
            Console.WriteLine("Hello, World!");
        }

        [Test]
        public void HelloWorld2()
        {
            Console.WriteLine("Hello, World!");
            Console.WriteLine("Hello, World!");
        }
    }

    public class AssumeTests
    {
        [Test]
        public void AssumeFalse()
        {
            Assume.That(false, "False assumption!");
        }
    }

    public class InconclusiveTests
    {
        [Test]
        public void Inconclusive()
        {
            Assert.Inconclusive("Oops!");
        }
    }

    public class ExceptionTests
    {
        [Test]
        public void ThrowException()
        {
            throw new Exception("Boom!");
        }
    }

    public class NoTestFixtureAttributeTests
    {
        [Test]
        public void Pass()
        {
        }
    }

    public static class StaticFixtureTests
    {
        [Test]
        public static void Pass()
        {
        }
    }

    [TestFixture(typeof(ArrayList))]
    [TestFixture(typeof(List<int>))]
    public class GenericFixtureTests<TList> where TList : IList, new()
    {
        [Test]
        public void CanAddToList()
        {
            Assert.AreEqual(typeof(ArrayList), typeof(TList));
        }
    }

    public class OuterClass
    {
        [TestFixture]
        public class NestedClassTests
        {
            [Test]
            public static void Pass()
            {
            }
        }
    }

    [TestFixture]
    public class FailTests
    {
        [Test]
        public void AreEqual_False()
        {
            Assert.AreEqual(1, 2);
        }
    }

    namespace MyNamespace
    {
        [TestFixture]
        public class Fix1
        {
            [Test]
            public void Test1()
            {
            }
        }

        [TestFixture]
        public class Fix2
        {
            [Test]
            public void Test1()
            {
            }
        }
    }

    [TestFixture]
    public class MockTestFixture
    {
        [Test]
        public void Test1()
        {
        }

        [Test]
        public void Test2()
        {
        }
    }

    [TestFixture]
    public class FailTestFixture
    {
        [Test]
        public void FailWithException()
        {
            throw new Exception();
        }

        [Test]
        public void FailWithAssert()
        {
            Assert.Fail("Boom!");
        }
    }

    [TestFixture]
    public class MockTestFixture1
    {
        [Test]
        public void Test1()
        {
        }
    }

    [TestFixture]
    public class IgnoredTests
    {
        [Test, Ignore("Ignore Me")]
        public void IgnoreAttributeTest()
        {
        }

        [Test]
        public void IgnoreAssertTest()
        {
            Assert.Ignore("Ignore Me");
        }
    }

    public class NoTests
    {
        public int NotAMethod;

        public void NoTest()
        {
        }
    }

    [TestFixture]
    public abstract class AbstractTests
    {
        [Test]
        public void Test1()
        {
        }

        [Test]
        public void Test2()
        {
        }
    }

    public class ConcreateTests1 : AbstractTests
    {
    }

    public class ConcreateTests2 : AbstractTests
    {
    }

    /// <test pass="0" fail="0" ignore="2" />
    [TestFixture]
    public class BadsigTests
    {
        /// <test pass="0" fail="0" ignore="1" />
        [Test]
        public int TestRetInt()
        {
            return 0;
        }

        /// <test pass="0" fail="0" ignore="1" />
        [Test]
        public void TestTakeInt(int i)
        {
        }
    }

    /// <test pass="0" fail="0" />
    [TestFixture]
    public class TestFixtureSetUpFail
    {
        // [TestFixtureSetUp]
        [OneTimeSetUp]
        public void TestFixtureSetUp()
        {
            throw new Exception("Boom!");
        }

        [Test]
        public void Test1()
        {
        }
    }

    namespace OverrideTests
    {
        abstract public class FixtureBase
        {
            public virtual void Test()
            {
            }
        }

        public class OverrideFixture : FixtureBase
        {
            [Test]
            public override void Test()
            {
                base.Test();
            }
        }
    }

    namespace FixtureSetUpTests
    {
        /// <test pass="1" fail="0" />
        [TestFixture]
        public class Fix1
        {
            [TestFixtureSetUp]
            public void TestFixtureSetUp()
            {
                Counters.TestFixtureSetUpCount++;
            }

            [Test]
            public void Test1()
            {
                Assert.AreEqual(1, Counters.TestFixtureSetUpCount);
            }
        }

        /// <test pass="1" fail="0" />
        [TestFixture]
        public class Fix2
        {
            [TestFixtureSetUp]
            public void TestFixtureSetUp()
            {
                Counters.TestFixtureSetUpCount++;
            }

            [Test]
            public void Test1()
            {
                Assert.AreEqual(1, Counters.TestFixtureSetUpCount);
            }
        }

        class Counters
        {
            internal static int TestFixtureSetUpCount;
        }
    }
}

public class NoNamespaceFixtureTests
{
    [NUnit.Framework.Test]
    public void Pass()
    {
    }
}
