using System;
using System.Diagnostics;
using System.IO;
using Frog.Domain.Underware;

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

        private void InitialCheckout()
        {
            var scriptPath = Path.Combine(GitScriptsLocation, "git_initial_fetch.bat");
            var process = ProcessHelpers.Start(scriptPath, _codeBase + " " + _repoFolder + " " + _repoUrl);
            Console.WriteLine(process.StandardOutput.ReadToEnd());
            process.WaitForExit();
            if (process.ExitCode != 0) {throw new InvalidProgramException("script failed, see log for details");}
        }

        public bool CheckForUpdates()
        {
            if (Directory.Exists(Path.Combine(Path.Combine(_codeBase,_repoFolder), ".git")))
            {
                var scriptPath = Path.Combine(GitScriptsLocation, "git_check_for_updates.bat");
                var process = ProcessHelpers.Start(scriptPath, _codeBase + " " + _repoFolder + " " + _repoUrl);
                process.WaitForExit();
                Console.WriteLine(process.StandardOutput.ReadToEnd());
                if (process.ExitCode != 0 && process.ExitCode != 201) throw new InvalidProgramException("script failed, see log for details");
                return process.ExitCode == 201;
            } else
            {
                InitialCheckout();
                return true;
            }
        }

        string GitScriptsLocation
        {
            get { return Path.Combine(Path.GetDirectoryName(GetType().Assembly.Location), "git_scripts"); }
        }

        public SourceDrop GetLatestSourceDrop(string sourceDropLocation)
        {
            CopyFolder(new DirectoryInfo(Path.Combine(_codeBase, _repoFolder)),new DirectoryInfo(sourceDropLocation));
            return new SourceDrop(sourceDropLocation);
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