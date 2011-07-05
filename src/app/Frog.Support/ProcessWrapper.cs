using System;
using System.Diagnostics;

namespace Frog.Support
{
    public class ProcessWrapper
    {
        public event Action<string> OnErrorOutput = s => Console.WriteLine(String.Format("E>{0}", s));
        public event Action<string> OnStdOutput = s => Console.WriteLine(String.Format("S>{0}", s));

        public ProcessWrapper(string cmdExe, string arguments) : this(cmdExe, arguments, System.IO.Directory.GetCurrentDirectory())
        {
        }

        public ProcessWrapper(string cmdExe, string arguments, string workingDirectory)
        {
            this.cmdExe = cmdExe;
            this.arguments = arguments;
            this.workingDirectory = workingDirectory;
        }

        public Process ProcessInfo  
        {
            get { return process; }
        }

        public void Execute()
        {
            var psi = new ProcessStartInfo(cmdExe, arguments)
            {
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                WorkingDirectory = workingDirectory
            };

            process = Process.Start(psi);
            process.ErrorDataReceived += (sender, args) => OnErrorOutput(args.Data);
            process.OutputDataReceived += (sender, args) => OnStdOutput(args.Data);

            process.BeginErrorReadLine();
            process.BeginOutputReadLine();
        }

        public int WaitForProcess()
        {
            process.WaitForExit();
            return process.ExitCode;
        }

        readonly string cmdExe;

        readonly string arguments;

        readonly string workingDirectory;

        Process process;

        public void WaitForProcess(int timeoutMilliseconds)
        {
            process.WaitForExit(timeoutMilliseconds);
        }
    }
}