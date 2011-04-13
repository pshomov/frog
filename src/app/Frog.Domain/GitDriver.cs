using System;
using System.IO;
using System.Text.RegularExpressions;
using Frog.Support;

namespace Frog.Domain
{
    public interface SourceRepoDriver
    {
        string GetLatestRevision();
        void GetSourceRevision(string revision, string checkoutPath);
    }

    public class GitDriver : SourceRepoDriver
    {
        const string RevisionExtractingRegex = @"^([a-f,0-9]*)\s*refs/heads/master";
        readonly string repoUrl;

        public GitDriver(string repo)
        {
            repoUrl = repo;
        }

        public string GetLatestRevision()
        {
            string scriptPath = Path.Combine(Underware.GitProductionScriptsLocation, "git_remote_latest_rev.rb");
            Console.WriteLine("Repo url is: "+repoUrl);
            var process = new ProcessWrapper("ruby",
                                             scriptPath + " \"" + repoUrl+"\"");
            string result = "";
            process.OnStdOutput +=
                s =>
                    {
                        if (result.Length == 0 && !s.IsNullOrEmpty())
                        {
                            if (Regex.IsMatch(s, RevisionExtractingRegex))
                                result = Regex.Match(s, RevisionExtractingRegex).Groups[1].Value;
                        }
                    };
            process.Execute();
            var exitcode = process.WaitForProcess();
            if (exitcode != 0)
                throw new InvalidProgramException("script failed, see log for details");
            return result;
        }

        public void GetSourceRevision(string revision, string workingArea)
        {
            var scriptPath = Path.Combine(Underware.GitProductionScriptsLocation, "git_fetch.rb");
            var process = new ProcessWrapper("ruby",
                                             scriptPath + " \"" + repoUrl + "\" " + revision + " " + " \"" + workingArea+"\"");
            process.Execute();
            var exitcode = process.WaitForProcess();
            if (exitcode != 0)
            {
                throw new InvalidProgramException("script failed, see log for details");
            }
        }
    }
}