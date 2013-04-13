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
        private string sub_folder_name = "sub";

        protected override void Given()
        {
            directory = GetTemporaryDirectory();
            var genesis = new FileGenesis(directory);
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

        string GetTemporaryDirectory()
        {
            string temp_directory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(temp_directory);
            return temp_directory;
        }
    }
}
