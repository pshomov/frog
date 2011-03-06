using System;
using System.IO;
using System.Text.RegularExpressions;
using Frog.Support;

namespace Frog.Domain
{
    public interface SourceRepoDriver
    {
        string GetLatestRevision(string repo);
        void GetSourceRevision(string repo, string revision);
        bool CheckForUpdates();
        SourceDrop GetLatestSourceDrop(string sourceDropLocation);
    }

    public class GitDriver : SourceRepoDriver
    {
        readonly string _codeBase;
        readonly string _repoFolder;
        readonly string _repoUrl;

        public GitDriver(string codeBase, string repoFolder, string repoUrl)
        {
            _codeBase = codeBase;
            _repoFolder = repoFolder;
            _repoUrl = repoUrl;
        }

        public string GetLatestRevision(string repo)
        {
            string scriptPath = Path.Combine(Underware.GitProductionScriptsLocation, "git_remote_latest_rev.rb");
            var process = new ProcessWrapper("ruby",
                                             scriptPath + " " + repo);
            string result = "";
            process.OnStdOutput +=
                s => { if (result.Length == 0)
                {
                    if (Regex.IsMatch(s, @"^([a-f,0-9]*)\s*refs/heads/master"))
                    result = Regex.Match(s, @"^([a-f,0-9]*)\s*refs/heads/master").Groups[1].Value;
                }
                        };
            process.Execute();
            var exitcode = process.WaitForProcess();
            if (exitcode != 0)
                throw new InvalidProgramException("script failed, see log for details");
            return result;
        }

        public void GetSourceRevision(string repo, string revision)
        {
            throw new NotImplementedException();
        }

        public bool CheckForUpdates()
        {
            if (Directory.Exists(Path.Combine(Path.Combine(_codeBase, _repoFolder), ".git")))
            {
                string scriptPath = Path.Combine(Underware.GitProductionScriptsLocation, "git_check_for_updates.rb");
                var process = new ProcessWrapper("ruby",
                                                 scriptPath + " " + _codeBase + " " + _repoFolder + " " + _repoUrl);
                process.Execute();
                var exitcode = process.WaitForProcess();
                if (exitcode != 0 && exitcode != 201)
                    throw new InvalidProgramException("script failed, see log for details");
                return exitcode == 201;
            }
            else
            {
                InitialCheckout();
                return true;
            }
        }

        public SourceDrop GetLatestSourceDrop(string sourceDropLocation)
        {
            CopyFolder(new DirectoryInfo(Path.Combine(_codeBase, _repoFolder)), new DirectoryInfo(sourceDropLocation));
            return new SourceDrop(sourceDropLocation);
        }

        void InitialCheckout()
        {
            string scriptPath = Path.Combine(Underware.GitProductionScriptsLocation, "git_initial_fetch.rb");
            var process = new ProcessWrapper("ruby",
                                             scriptPath + " " + _codeBase + " " + _repoFolder + " " + _repoUrl);
            process.Execute();
            var exitcode = process.WaitForProcess();
            if (exitcode != 0)
            {
                throw new InvalidProgramException("script failed, see log for details");
            }
        }

        static void CopyFolder(DirectoryInfo source, DirectoryInfo target)
        {
            foreach (DirectoryInfo dir in source.GetDirectories())
                if (dir.Name != ".git") CopyFolder(dir, target.CreateSubdirectory(dir.Name));
            foreach (FileInfo file in source.GetFiles())
                file.CopyTo(Path.Combine(target.FullName, file.Name));
        }
    }
}