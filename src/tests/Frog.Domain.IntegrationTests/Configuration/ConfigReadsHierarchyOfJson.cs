using System.IO;
using Frog.Specs.Support;
using Frog.Support;
using NUnit.Framework;

namespace Frog.Domain.IntegrationTests.Configuration
{
    class ConfigReadsHierarchyOfJson : BDD
    {
        private dynamic config;
        private string directory;
        private string sub_folder_name = "sub";

        protected override void Given()
        {
            directory = GetTemporaryDirectory();
            var genesis = new FileGenesis(directory);
            genesis.File("config.json", "{setting1 : 'v1', setting2 : 'wow' }")
                   .Folder(sub_folder_name)
                   .File("config.json", "{setting1 : 'v2'}");
        }

        protected override void When()
        {
            config = new Config(Path.Combine(directory, sub_folder_name));
        }

        [Test]
        public void should_use_the_setting_value_thats_closest_to_the_app()
        {
            Assert.That(config.setting1, Is.EqualTo("v2"));
        }

        [Test]
        public void should_use_the_setting_value_found_in_parent_when_no_value_present_locally()
        {
            Assert.That(config.setting2, Is.EqualTo("wow"));
        }

        string GetTemporaryDirectory()
        {
            string temp_directory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(temp_directory);
            return temp_directory;
        }
    }

}
