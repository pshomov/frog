using System.IO;
using NUnit.Framework;

namespace Frog.Domain.Specs
{
    [TestFixture]
    public class GitRepositoryDriverCheckingForUpdates : BDD
    {
        GitDriver _driver;
        string _testAssemblyPath;
        bool _updates;

        public override void Given()
        {
            _testAssemblyPath = Path.GetDirectoryName(GetType().Assembly.Location);
            var repo = GitTestSupport.CreateDummyRepo(_testAssemblyPath, "dummy_repo");
            GitTestSupport.CommitChange(_testAssemblyPath, "dummy_repo");
            _driver = new GitDriver(_testAssemblyPath, "tmp_folder", repo);
        }

        public override void When()
        {
            _updates = _driver.CheckForUpdates();
        }

        [Test]
        public void should_report_that_there_are_updates()
        {
            Assert.That(_updates);
        }
    }
}