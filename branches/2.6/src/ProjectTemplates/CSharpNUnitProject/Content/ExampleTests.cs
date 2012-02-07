namespace $safeprojectname$
{
    using System;
    using System.Threading;
    using NUnit.Framework;

    public class ExampleTests
    {
        [Test]
        public void Add()
        {
            Assert.That(1 + 2, Is.EqualTo(3));
        }

        [TestCase(1, 2, 3)]
        [TestCase(2, 2, 4)]
        public void Add(int x, int y, int result)
        {
            Assert.That(x + y, Is.EqualTo(result));
        }

        [Test]
        public void Throws()
        {
            Assert.Throws<ApplicationException>(
                delegate() { throw new ApplicationException(); });
        }

        [Test, SetCulture("fr-FR")]
        public void SetCulture()
        {
            Assert.That(Convert.ToDateTime("1 Mars").Month, Is.EqualTo(3));
        }

        [Test, RequiresMTA]
        public void RequiresMTA()
        {
            Assert.That(Thread.CurrentThread.GetApartmentState(),
                Is.EqualTo(ApartmentState.MTA));
        }

        [Test, RequiresSTA]
        public void RequiresSTA()
        {
            Assert.That(Thread.CurrentThread.GetApartmentState(),
                Is.EqualTo(ApartmentState.STA));
        }
    }
}
