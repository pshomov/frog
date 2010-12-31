using System.IO;
using NUnit.Framework;

namespace Frog.Domain.Specs.Git
{
    [TestFixture]
    public class GitRepositoryDriverGetSourceDrop : GitRepositoryDriverCheckBase
    {
        string _srcDropLocation;

        public override void Given()
        {
            base.Given();
            _srcDropLocation = Path.Combine(_workPlace, "srcDrop");
            Directory.CreateDirectory(_srcDropLocation);
            _driver.CheckForUpdates();
        }

        public override void When()
        {
            _driver.GetLatestSourceDrop(_srcDropLocation);
        }

        [Test]
        public void should_place_source_code_in_specified_location()
        {
            Assert.That(Directory.Exists(_srcDropLocation));
            Assert.That(File.Exists(Path.Combine(_srcDropLocation,"test.txt")));
        }

        [Test]
        public void should_not_copy_over_dotgit_folder()
        {
            Assert.That(!Directory.Exists(Path.Combine(_srcDropLocation, ".git")));
        }
    }
}