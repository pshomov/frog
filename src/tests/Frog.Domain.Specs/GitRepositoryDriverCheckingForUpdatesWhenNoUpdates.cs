using System.IO;
using NUnit.Framework;

namespace Frog.Domain.Specs
{
    [TestFixture]
    public class GitRepositoryDriverCheckingForUpdatesWhenNoUpdates : BDD
    {
        GitDriver _driver;
        string _testAssemblyPath;
        bool _updates;

        public override void Given()
        {
            _testAssemblyPath = Path.GetTempPath() + "\\"+Path.GetRandomFileName();
            Directory.CreateDirectory(_testAssemblyPath);
            var repo = GitTestSupport.CreateDummyRepo(_testAssemblyPath, "dummy_repo");
            _driver = new GitDriver(_testAssemblyPath, "tmp_folder", repo);
            _driver.InitialCheckout();
        }

        public override void When()
        {
            _updates = _driver.CheckForUpdates();
        }

        [Test]
        public void should_report_that_there_are_no_updates()
        {
            Assert.That(!_updates);
        }
    }
}