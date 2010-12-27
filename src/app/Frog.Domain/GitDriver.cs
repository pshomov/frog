using System;
using System.Diagnostics;
using System.IO;
using Frog.Domain.Underware;

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
            var scriptPath = GitScriptsLocation + "\\git_initial_fetch.bat";
            var process = ProcessHelpers.Start(scriptPath, _codeBase + " " + _repoFolder + " " + _repoUrl);
            Console.WriteLine(process.StandardOutput.ReadToEnd());
            process.WaitForExit();
            if (process.ExitCode != 0) {throw new InvalidProgramException("script failed, see log for details");}
        }

        public bool CheckForUpdates()
        {
            var scriptPath = GitScriptsLocation + "\\git_check_for_updates.bat";
            var process = ProcessHelpers.Start(scriptPath, _codeBase + " " + _repoFolder + " " + _repoUrl);
            process.WaitForExit();
            Console.WriteLine(process.StandardOutput.ReadToEnd()); 
            if (process.ExitCode != 0 && process.ExitCode != 201) throw new InvalidProgramException("script failed, see log for details");
            return process.ExitCode == 201;
        }

        string GitScriptsLocation
        {
            get { return Path.GetDirectoryName(GetType().Assembly.Location)+"\\git_scripts"; }
        }
    }
}