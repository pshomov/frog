using System.IO;
using Frog.Specs.Support;
using Frog.Support;
using NUnit.Framework;

namespace Frog.Domain.IntegrationTests.Configuration
{
    class ConfigReadsNestedSettings : BDD
    {
        private dynamic config;
        private string directory;
        private FileGenesis genesis;

        protected override void Given()
        {
            directory = IO.GetTemporaryDirectory();
            genesis = new FileGenesis(directory);
            genesis.File("config.json", "{level1 : {level2 : '1', level21 : {level3 : 'level3'}} }");
        }

        protected override void When()
        {
            config = new Config(Path.Combine(directory, directory));
        }

        [Test]
        public void should_reach_level2()
        {
            Assert.That(config.level1.level2, Is.EqualTo("1"));
        }

        [Test]
        public void should_reach_level3()
        {
            Assert.That(config.level1.level21.level3, Is.EqualTo("level3"));
        }
    }
}
