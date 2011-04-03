using System;
using System.IO;
using System.Linq;
using Frog.Support;

namespace Frog.Domain.Specs
{
    public class GitTestSupport
    {
        public static string GetTempPath()
        {
            return Path.GetTempPath();
        }

        public static string CreateDummyRepo(string basePath, string repoName)
        {
            var path = Path.Combine(Underware.GitSupportScriptsLocation, "git_create_dummy_repo.rb");
            var process = new ProcessWrapper("ruby", path + " " + basePath + " " + repoName);
            process.Execute();
            process.WaitForProcess();
            return Path.Combine(basePath, repoName);
        }

        public static void CommitChangeFiles(string repo, string fileset, string commitMessage = "wind of change")
        {
            var scriptPath = Path.Combine(Underware.GitSupportScriptsLocation, "git_commit_files.rb");
            var process = new ProcessWrapper("ruby", scriptPath + " " + repo + " " + fileset+"/." + " "+commitMessage);
            process.Execute();
            process.WaitForProcess();
        }
    }
}