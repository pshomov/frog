using System.Diagnostics;

namespace Frog.Domain.Underware
{
    public class ProcessHelpers
    {
        public static Process Start(string path, string arguments)
        {
            var psi = new ProcessStartInfo(path, arguments);
            psi.RedirectStandardOutput = true;
            psi.RedirectStandardError = true;
            psi.UseShellExecute = false;
            return Process.Start(psi);
        }
    }
}