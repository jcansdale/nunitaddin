using System;
using NUnit.Framework;

namespace NUnit.AddInRunner.Tests
{
    public class NUnitConfigTests
    {
        [Test]
        public void Load()
        {
            NUnitConfig config = NUnitConfig.Load("nunit.config");
            Assert.That(config.Infos.Length, Is.EqualTo(2));
            Assert.That(config.Infos[0].RuntimeVersion, Is.EqualTo("v1.1.4322"));
            Assert.That(config.Infos[0].BaseDir, Is.EqualTo("net-1.1"));
            Assert.That(config.Infos[1].RuntimeVersion, Is.EqualTo("v2.0.50727"));
            Assert.That(config.Infos[1].BaseDir, Is.EqualTo("net-2.0"));
        }
    }
}
