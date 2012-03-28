using System.IO;
using Frog.Domain;
using Frog.Domain.ExecTasks;
using Frog.Support;

namespace Frog.Specs.Support
{
    public static class GitTestSupport
    {
        public static string GetTempPath()
        {
            return Path.GetTempPath();
        }

        public static string CreateDummyRepo(string basePath, string repoName)
        {
            var path = Path.Combine(Underware.GitSupportScriptsLocation, "git_create_dummy_repo.rb");
            var process = new ExecTask("ruby", path + " \"" + basePath + "\" " + repoName, "Git create dummy repo",(s, s1, arg3) => new ProcessWrapper(s,s1,arg3 ));
            process.Perform(new SourceDrop(Directory.GetCurrentDirectory()));
            return Path.Combine(basePath, repoName);
        }

        public static void CommitChangeFiles(string repo, string fileset, string commitMessage = "wind of change")
        {
            var scriptPath = Path.Combine(Underware.GitSupportScriptsLocation, "git_commit_files.rb");
            var process = new ExecTask("ruby", scriptPath + " \"" + repo + "\" \"" + fileset + "/.\" \"" + commitMessage+"\"", "Commit all changes to git", (s, s1, arg3) => new ProcessWrapper(s, s1, arg3));
            process.Perform(new SourceDrop(Directory.GetCurrentDirectory()));
        }
    }
}