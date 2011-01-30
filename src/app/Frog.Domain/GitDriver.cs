using System;
using System.Diagnostics;
using System.IO;
using Frog.Support;

namespace Frog.Domain
{
    public interface SourceRepoDriver
    {
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

        static string GitScriptsLocation
        {
            get
            {
                return Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                                    (Underware.IsWebApp() ? "bin"+Path.DirectorySeparatorChar : "") + "git_scripts");
            }
        }

        public bool CheckForUpdates()
        {
            if (Directory.Exists(Path.Combine(Path.Combine(_codeBase, _repoFolder), ".git")))
            {
                string scriptPath = Path.Combine(GitScriptsLocation, "git_check_for_updates.rb");
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
            string scriptPath = Path.Combine(GitScriptsLocation, "git_initial_fetch.rb");
            var process = new ProcessWrapper("ruby",
                                                   scriptPath + " " + _codeBase + " " + _repoFolder + " " + _repoUrl);
            process.Execute();
            var exitcode = process.WaitForProcess();
            if (exitcode != 0)
            {
                throw new InvalidProgramException("script failed, see log for details");
            }
        }

        public static void CopyFolder(DirectoryInfo source, DirectoryInfo target)
        {
            foreach (DirectoryInfo dir in source.GetDirectories())
                if (dir.Name != ".git") CopyFolder(dir, target.CreateSubdirectory(dir.Name));
            foreach (FileInfo file in source.GetFiles())
                file.CopyTo(Path.Combine(target.FullName, file.Name));
        }
    }
}