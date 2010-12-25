using System;
using System.ComponentModel;
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
            _driver = new GitDriver(_testAssemblyPath, "tmp_folder", "git://github.com/pshomov/xray.git");
        }

        public void When()
        {
            _driver.InitialCheckout();
        }
        [Test]
        public void should_create_repo_folder()
        {
            Assert.That(System.IO.Directory.Exists(_testAssemblyPath+"\\tmp_folder"));
        }
    }

    public class GitDriver
    {
        readonly string _codeBase;
        readonly string _repoFolder;
        readonly string _repoUrl;

        public GitDriver(string codeBase, string repoFolder, string repoUrl)
        {
            _codeBase = codeBase;
            _repoFolder = repoFolder;
            _repoUrl = repoUrl;
        }

        public void InitialCheckout()
        {
            var path = _codeBase + "\\git_scripts\\git_initial_fetch.bat";
            var process = Process.Start(path, _codeBase + " " + _repoFolder + " " + _repoUrl);
            process.WaitForExit();
        }
    }
}