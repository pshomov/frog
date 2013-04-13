using System.IO;
using Frog.Specs.Support;
using Frog.Support;
using NUnit.Framework;

namespace Frog.Domain.IntegrationTests.Configuration
{
    class ConfigWorksEvenWhenNoJsonPresent : BDD
    {
        private dynamic config;
        private string directory;

        protected override void Given()
        {
            directory = GetTemporaryDirectory();
        }

        protected override void When()
        {
            config = new Config(directory);
        }

        [Test]
        public void should_have_settings_not_defined()
        {
            try
            {
                var a = config.setting1;
            }
            catch (SettingNotDefined)
            {
            }
        }

        string GetTemporaryDirectory()
        {
            string temp_directory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(temp_directory);
            return temp_directory;
        }
    }

}
