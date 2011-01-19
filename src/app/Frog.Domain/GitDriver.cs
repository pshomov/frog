using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
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
            get { return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, (ProcessHelpers.IsWebApp() ? "bin\\" : "") + "git_scripts"); }
        }

        public bool CheckForUpdates()
        {
            if (Directory.Exists(Path.Combine(Path.Combine(_codeBase, _repoFolder), ".git")))
            {
                string scriptPath = Path.Combine(GitScriptsLocation, "git_check_for_updates.rb");
                Process process = ProcessHelpers.Start("ruby",
                                                       scriptPath + " " + _codeBase + " " + _repoFolder + " " + _repoUrl);
                Console.WriteLine(process.StandardOutput.ReadToEnd());
                Console.WriteLine("Errors: " + process.StandardError.ReadToEnd());
                process.WaitForExit();
                if (process.ExitCode != 0 && process.ExitCode != 201)
                    throw new InvalidProgramException("script failed, see log for details");
                return process.ExitCode == 201;
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
            Process process = ProcessHelpers.Start("ruby",
                                                   scriptPath + " " + _codeBase + " " + _repoFolder + " " + _repoUrl);
            string stdout_buffer = process.StandardOutput.ReadToEnd();
            Console.WriteLine(stdout_buffer);
            string stderr_buffer = process.StandardError.ReadToEnd();
            Console.WriteLine(stderr_buffer);
            process.WaitForExit();
            if (process.ExitCode != 0)
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