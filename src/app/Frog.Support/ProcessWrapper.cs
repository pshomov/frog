using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;

namespace Frog.Support
{
    public class ProcessWrapper
    {
        public event Action<string> OnErrorOutput = s => Console.WriteLine(String.Format("E>{0}", s));
        public event Action<string> OnStdOutput = s => Console.WriteLine(String.Format("S>{0}", s));

        public ProcessWrapper(string cmdExe, string arguments) : this(cmdExe, arguments, Directory.GetCurrentDirectory())
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

        public TimeSpan TotalProcessorTime
        {
            get {
				if (Os.IsUnix){
					var p = new ProcessWrapper("ps", string.Format("-p {0} -o time=", process.Id));
					var time = "";
					p.OnStdOutput += s => time = s ?? "";
					p.Execute();
					p.WaitForProcess();
					if (time.Length > 0){
						return TimeSpan.Parse(time);
					} else return new TimeSpan(0);
				} else
				return process.TotalProcessorTime; 
			}
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

            try
            {
                process = Process.Start(psi);
            }
            catch (Win32Exception e)
            {
                throw new ApplicationNotFoundException(cmdExe, e);
            }
            process.ErrorDataReceived += (sender, args) => OnErrorOutput(args.Data);
            process.OutputDataReceived += (sender, args) => OnStdOutput(args.Data);

            process.BeginErrorReadLine();
            process.BeginOutputReadLine();
        }

        public int WaitForProcess()
        {
            process.WaitForExit();
            MonoBugFix(process.ExitCode);
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

        private static void MonoBugFix(int exitcode)
        {
            if (exitcode == 1) throw new Win32Exception();
        }

        public void Kill()
        {
            try
            {
                process.Kill();
            }
            catch (InvalidOperationException e)
            {
            }
        }
    }

    public class ApplicationNotFoundException : Exception
    {
        public ApplicationNotFoundException(string application, Exception inner) : base(string.Format("Error launching application {0}", application), inner)
        {
        }
    }
}