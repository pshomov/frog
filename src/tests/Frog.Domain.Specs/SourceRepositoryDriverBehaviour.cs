using System.Diagnostics;
using System.IO;
using Frog.Domain.SourceRepositories;
using NUnit.Framework;

namespace Frog.Domain.Specs
{
    [TestFixture]
    public class SourceRepositoryDriverBehaviour : BDD
    {
        GitDriver _driver;
        string _testAssemblyPath;

        public override void Given()
        {
            _testAssemblyPath = Path.GetTempPath() + "\\" + Path.GetRandomFileName();
            Directory.CreateDirectory(_testAssemblyPath);
            var repo = GitTestSupport.CreateDummyRepo(_testAssemblyPath, "dummy_repo");
            _driver = new GitDriver(_testAssemblyPath, "tmp_folder", repo);
        }

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