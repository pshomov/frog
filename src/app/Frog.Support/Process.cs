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


        public static Process Start(string path, string arguments, string startupDir)
        {
            var psi = new ProcessStartInfo(path, arguments)
                          {
                              RedirectStandardOutput = true,
                              RedirectStandardError = true,
                              UseShellExecute = false,
                              WorkingDirectory = startupDir
                          };
            return Process.Start(psi);
        }
    }
}