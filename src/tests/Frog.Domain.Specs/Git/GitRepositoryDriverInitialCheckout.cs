using System.Diagnostics;
using System.IO;
using Frog.Domain.SourceRepositories;
using NUnit.Framework;

namespace Frog.Domain.Specs
{
    [TestFixture]
    public class GitRepositoryDriverInitialCheckout : GitRepositoryDriverCheckBase
    {
        public override void When()
        {
            _driver.CheckForUpdates();
        }

        [Test]
        public void should_create_repo_folder()
        {
            Assert.That(Directory.Exists(_testAssemblyPath+"\\tmp_folder"));
        }

        [Test]
        public void should_have_the_repo_contents_checked_out()
        {
            Assert.That(File.Exists(_testAssemblyPath + "\\tmp_folder\\test.txt"));
        }

    }
}