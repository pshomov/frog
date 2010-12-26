using System;
using System.Diagnostics;

namespace Frog.Domain.Specs
{
    public class GitTestSupport
    {
        public static string CreateDummyRepo(string basePath, string repoName)
        {
            var path = basePath + "\\git_support_scripts\\git_create_dummy_repo.bat";
            var process = Process.Start(path, basePath + " " + repoName);
            process.WaitForExit();
            return basePath + "\\" + repoName;
        }

        public static void CommitChange(string basePath, string repoName)
        {
            var path = basePath + "\\git_support_scripts\\git_commit_change.bat";
            var process = Process.Start(path, basePath + " " + repoName);
            process.WaitForExit();
        }
    }
}