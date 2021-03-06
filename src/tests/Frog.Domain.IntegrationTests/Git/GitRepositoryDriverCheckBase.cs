using System.IO;
using Frog.Domain.Integration;
using Frog.Specs.Support;
using Frog.Support;

namespace Frog.Domain.IntegrationTests.Git
{
    public abstract class GitRepositoryDriverCheckBase : BDD
    {
        protected const string _repoFolder = "dummy_repo";
        protected const string _cloneFolder = "tmp_folder";
        protected GitDriver _driver;
        protected string _workPlace;
        protected string repoUrl;

        protected override void Given()
        {
            _workPlace = Path.Combine(GitTestSupport.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(_workPlace);
            repoUrl = GitTestSupport.CreateDummyRepo(_workPlace, _repoFolder);
            _driver = new GitDriver(repoUrl);
        }

        protected override void GivenCleanup()
        {
            OSHelpers.ClearAttributes(_workPlace);
            Directory.Delete(_workPlace, true);
        }
    }
}