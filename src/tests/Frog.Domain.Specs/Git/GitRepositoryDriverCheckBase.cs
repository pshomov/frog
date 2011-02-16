using System.IO;
using System.Threading;
using Frog.Specs.Support;

namespace Frog.Domain.Specs.Git
{
    public abstract class GitRepositoryDriverCheckBase : BDD
    {
        protected const string _repoFolder = "dummy_repo";
        protected const string _cloneFolder = "tmp_folder";
        protected GitDriver _driver;
        protected string _workPlace;
        protected string repoUrl;

        public override void Given()
        {
            _workPlace = Path.Combine(GitTestSupport.GetTempPath() ,Path.GetRandomFileName());
            Directory.CreateDirectory(_workPlace);
            repoUrl = GitTestSupport.CreateDummyRepo(_workPlace, _repoFolder);
            _driver = new GitDriver(_workPlace, _cloneFolder, repoUrl);
        }

        protected override void GivenCleanup()
        {
            OSHelpers.ClearAttributes(_workPlace);
            Directory.Delete(_workPlace, true);
        }
    }
}