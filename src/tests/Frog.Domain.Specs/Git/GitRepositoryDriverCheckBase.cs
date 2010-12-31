using System.IO;

namespace Frog.Domain.Specs.Git
{
    public abstract class GitRepositoryDriverCheckBase : BDD
    {
        protected const string _repoFolder = "dummy_repo";
        protected const string _cloneFolder = "tmp_folder";
        protected GitDriver _driver;
        protected string _workPlace;

        public override void Given()
        {
            _workPlace = Path.Combine(Path.GetTempPath() ,Path.GetRandomFileName());
            Directory.CreateDirectory(_workPlace);
            var repo = GitTestSupport.CreateDummyRepo(_workPlace, _repoFolder);
            _driver = new GitDriver(_workPlace, _cloneFolder, repo);
        }

        public void Cleanup()
        {
            Directory.Delete(_workPlace, true);
        }
    }
}