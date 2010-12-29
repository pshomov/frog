using System.IO;
using Frog.Domain.SourceRepositories;
using NUnit.Framework;

namespace Frog.Domain.Specs.Git
{
    [TestFixture]
    public class GitRepositoryDriverGetSourceDrop : GitRepositoryDriverCheckBase
    {
        string _checkoutLocation;

        public override void Given()
        {
            base.Given();
            _checkoutLocation = _testAssemblyPath + "\\" + "srcDrop";
            Directory.CreateDirectory(_checkoutLocation);
            _driver.CheckForUpdates();
        }

        public override void When()
        {
            _driver.GetLatestSourceDrop(_checkoutLocation);
        }

        [Test]
        public void should_place_source_code_in_specified_location()
        {
            Assert.That(Directory.Exists(_checkoutLocation));
            Assert.That(File.Exists(_checkoutLocation+"\\test.txt"));
        }

    }
}