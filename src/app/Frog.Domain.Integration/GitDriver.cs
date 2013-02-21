using System;
using System.IO;
using System.Text.RegularExpressions;
using Frog.Support;

namespace Frog.Domain.Integration
{
    public class GitDriver : SourceRepoDriver
    {
        const string RevisionExtractingRegex = @"^([a-f,0-9]*)\s*refs/heads/master";
        readonly string repoUrl;
        public const int GitTimeoutInMs = 120000;

        public GitDriver(string repo)
        {
            repoUrl = repo;
        }

        public RevisionInfo GetLatestRevision()
        {
            string scriptPath = Path.Combine(Underware.GitProductionScriptsLocation, "git_remote_latest_rev.rb");
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
            process.WaitForProcess(GitTimeoutInMs);
            var exitcode = process.Dispose();
            if (exitcode != 0)
                throw new InvalidProgramException("script failed, see log for details");
            if (result.IsNullOrEmpty()) 
                throw new InvalidProgramException("Failed to retrieve repo revision");
            return new RevisionInfo {Revision = result};
        }

        public CheckoutInfo GetSourceRevision(string revision, string workingArea)
        {
            var scriptPath = Path.Combine(Underware.GitProductionScriptsLocation, "git_fetch.rb");
            var process = new ProcessWrapper("ruby",
                                             scriptPath + " \"" + repoUrl + "\" " + revision + " " + " \"" + workingArea+"\"");
            var log = "";
            process.OnStdOutput += s => { if (!s.IsNullOrEmpty()) log = s; };
            process.Execute();
            process.WaitForProcess(GitTimeoutInMs);
            var exitcode = process.Dispose();
            if (exitcode != 0)
                throw new InvalidProgramException("script failed, see log for details");
            return new CheckoutInfo {Comment = log, Revision = revision};
        }
    }
}