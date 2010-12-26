using System.Diagnostics;
using System.IO;
using NUnit.Framework;

namespace Frog.Domain.Specs
{
    [TestFixture]
    public class SourceRepositoryDriverBehaviour
    {
        GitDriver _driver;
        string _testAssemblyPath;

        [SetUp]
        public void Setup()
        {
            Given();
            When();
        }

        public void Given()
        {
            _testAssemblyPath = Path.GetDirectoryName(GetType().Assembly.Location);
            var repo = CreateDummyRepo(_testAssemblyPath, "dummy_repo");
            _driver = new GitDriver(_testAssemblyPath, "tmp_folder", repo);
        }

        public void When()
        {
            _driver.InitialCheckout();
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

        string CreateDummyRepo(string basePath, string repoName)
        {
            var path = basePath + "\\git_support_scripts\\git_create_dummy_repo.bat";
            var process = Process.Start(path, basePath + " " + repoName);
            process.WaitForExit();
            return basePath + "\\" + repoName;
        }
    }
}