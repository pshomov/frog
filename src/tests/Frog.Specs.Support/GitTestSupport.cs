using System;
using System.IO;
using System.Linq;
using Frog.Support;

namespace Frog.Domain.Specs
{
    public class GitTestSupport
    {
        public static string GitScriptsLocation
        {
            get { return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, (Underware.IsWebApp() ? "bin"+Path.DirectorySeparatorChar : "") + "git_support_scripts"); }
        }

        public static string CreateDummyRepo(string basePath, string repoName)
        {
            var path = Path.Combine(GitScriptsLocation, "git_create_dummy_repo.rb");
            var process = new ProcessWrapper("ruby", path + " " + basePath + " " + repoName);
            process.Execute();
            process.WaitForProcess();
            return Path.Combine(basePath, repoName);
        }

        public static void CommitChangeFiles(string repo, string fileset)
        {
            var path = Path.Combine(GitScriptsLocation, "git_commit_files.rb");
            var process = new ProcessWrapper("ruby", path + " " + repo + " " + fileset);
            process.Execute();
            process.WaitForProcess();
        }
    }
}