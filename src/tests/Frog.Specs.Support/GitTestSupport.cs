using System;
using System.Diagnostics;
using System.IO;
using Frog.Domain.Underware;

namespace Frog.Domain.Specs
{
    public class GitTestSupport
    {
        public static string CreateDummyRepo(string basePath, string repoName)
        {
			string path;
			if (Underware.IsWindows)
            	path = Path.Combine(Path.GetDirectoryName(typeof(GitTestSupport).Assembly.Location), "git_support_scripts\\git_create_dummy_repo.bat");
			else
            	path = Path.Combine(Path.GetDirectoryName(typeof(GitTestSupport).Assembly.Location), "git_support_scripts/git_create_dummy_repo.rb");
            Process process = ProcessHelpers.Start(path, basePath + " " + repoName);
            Console.WriteLine(process.StandardOutput.ReadToEnd());
            Console.WriteLine("Error out:\n"+process.StandardError.ReadToEnd());
            process.WaitForExit();
            return Path.Combine(basePath, repoName);
        }

        public static void CommitChange(string basePath, string repoName)
        {
			string path;
			if (Underware.IsWindows)
            	path = Path.Combine(Path.GetDirectoryName(typeof(GitTestSupport).Assembly.Location), "git_support_scripts\\git_commit_change.bat");
			else
            	path = Path.Combine(Path.GetDirectoryName(typeof(GitTestSupport).Assembly.Location), "git_support_scripts/git_commit_change.rb");
            var process = ProcessHelpers.Start(path, basePath + " " + repoName);
            Console.WriteLine(process.StandardOutput.ReadToEnd());
            process.WaitForExit();
        }
    }
}