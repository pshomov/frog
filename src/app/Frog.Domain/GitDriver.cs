using System;
using System.Diagnostics;

namespace Frog.Domain
{
    public class GitDriver
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

        public void InitialCheckout()
        {
            var path = _codeBase + "\\git_scripts\\git_initial_fetch.bat";
            var process = Process.Start(path, _codeBase + " " + _repoFolder + " " + _repoUrl);
            process.WaitForExit();
        }

        public bool CheckForUpdates()
        {
            var path = _codeBase + "\\git_scripts\\git_check_for_updates.bat";
            var process = Process.Start(path, _codeBase + " " + _repoFolder + " " + _repoUrl);
            process.WaitForExit();
            return process.ExitCode == 1;
        }
    }
}