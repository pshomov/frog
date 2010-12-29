using System.IO;
using Frog.Domain.SourceRepositories;

namespace Frog.Domain.Specs
{
    public abstract class GitRepositoryDriverCheckBase : BDD
    {
        protected GitDriver _driver;
        protected string _workPlace;

        public override void Given()
        {
            _workPlace = Path.GetTempPath() + "\\" + Path.GetRandomFileName();
            Directory.CreateDirectory(_workPlace);
            var repo = GitTestSupport.CreateDummyRepo(_workPlace, "dummy_repo");
            _driver = new GitDriver(_workPlace, "tmp_folder", repo);
        }

        public void Cleanup()
        {
            Directory.Delete(_workPlace, true);
        }
    }
}