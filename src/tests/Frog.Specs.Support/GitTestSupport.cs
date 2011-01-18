using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Frog.Domain.Underware;

namespace Frog.Domain.Specs
{
    public class GitTestSupport
    {
        static bool IsWebApp()
        {
            return AppDomain.CurrentDomain.GetAssemblies().Any(assembly => assembly.FullName == "System.Web.Mvc");
        }

        static string GitScriptsLocation
        {
            get { return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, (IsWebApp() ? "bin\\" : "") + "git_support_scripts"); }
        }

        public static string CreateDummyRepo(string basePath, string repoName)
        {
            var path = Path.Combine(GitScriptsLocation, "git_create_dummy_repo.rb");
            var process = ProcessHelpers.Start("ruby", path + " " + basePath + " " + repoName);
            Console.WriteLine(process.StandardOutput.ReadToEnd());
            Console.WriteLine("Error out:\n"+process.StandardError.ReadToEnd());
            process.WaitForExit();
            return Path.Combine(basePath, repoName);
        }

        public static void CommitChange(string basePath, string repoName)
        {
            var path = Path.Combine(GitScriptsLocation, "git_commit_change.rb");
            var process = ProcessHelpers.Start("ruby", path + " " + basePath + " " + repoName);
            Console.WriteLine(process.StandardOutput.ReadToEnd());
            process.WaitForExit();
        }
    }
}