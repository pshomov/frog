using System.IO;
using Frog.Domain.SourceRepositories;

namespace Frog.Domain.Specs
{
    public abstract class GitRepositoryDriverCheckBase : BDD
    {
        protected GitDriver _driver;
        protected string _testAssemblyPath;
        public override void Given()
        {
            _testAssemblyPath = Path.GetTempPath() + "\\" + Path.GetRandomFileName();
            Directory.CreateDirectory(_testAssemblyPath);
            var repo = GitTestSupport.CreateDummyRepo(_testAssemblyPath, "dummy_repo");
            _driver = new GitDriver(_testAssemblyPath, "tmp_folder", repo);
        }
    }
}