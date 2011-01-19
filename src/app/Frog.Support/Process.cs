using System;
using System.Diagnostics;
using System.Linq;

namespace Frog.Support
{
    public static class ProcessHelpers
    {
        public static bool IsWebApp()
        {
            return AppDomain.CurrentDomain.GetAssemblies().Any(
                assembly => assembly.FullName.StartsWith("System.Web.Mvc,"));
        }

        public static Process Start(string path, string arguments)
        {
            var psi = new ProcessStartInfo(path, arguments);
            psi.RedirectStandardOutput = true;
            psi.RedirectStandardError = true;
            psi.UseShellExecute = false;
            return Process.Start(psi);
        }

        public static Process Start(string path, string arguments, string startupDir)
        {
            var psi = new ProcessStartInfo(path, arguments);
            psi.RedirectStandardOutput = true;
            psi.RedirectStandardError = true;
            psi.UseShellExecute = false;
            psi.WorkingDirectory = startupDir;
            return Process.Start(psi);
        }
    }
}