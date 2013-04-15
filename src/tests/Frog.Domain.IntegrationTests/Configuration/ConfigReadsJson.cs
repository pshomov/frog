using Frog.Specs.Support;
using Frog.Support;
using NUnit.Framework;

namespace Frog.Domain.IntegrationTests.Configuration
{
    class ConfigReadsJson : BDD
    {
        private dynamic config;
        private string directory;

        protected override void Given()
        {
            directory = IO.GetTemporaryDirectory();
            var genesis = new FileGenesis(directory);
            genesis.File("config.json", "{setting1 : 'v1'}");
        }

        protected override void When()
        {
            config = new Config(directory);
        }

        [Test]
        public void should_read_the_value_from_the_config_when_accessed_as_property()
        {
            Assert.That(config.setting1, Is.EqualTo("v1"));
        }

        [Test]
        public void should_read_the_value_from_the_config_when_accessed_as_index()
        {
            Assert.That(config["setting1"], Is.EqualTo("v1"));
        }

        [Test]
        public void should_throw_exception_when_setting_not_defined()
        {
            try
            {
                var a = config.setting2;
            }
            catch (SettingNotDefined)
            {
            }
        }
    }

}
