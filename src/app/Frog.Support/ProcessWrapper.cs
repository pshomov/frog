using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Frog.Support
{
    public interface IProcessWrapper
    {
        string ProcessTreeCPUUsageId { get; }
        int Id { get; }
        event Action<string> OnErrorOutput;
        event Action<string> OnStdOutput;
        void Execute();
        bool WaitForProcess(int timeoutMilliseconds);
        int Dispose();
    }

    public class ProcessWrapper : IProcessWrapper
    {
        private readonly string arguments;
        private readonly string cmdExe;
        private readonly string workingDirectory;

        private Process process;

        public ProcessWrapper(string cmdExe, string arguments)
            : this(cmdExe, arguments, Directory.GetCurrentDirectory())
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

        public event Action<string> OnErrorOutput = s => {};
        public event Action<string> OnStdOutput = s => {};

        public string ProcessTreeCPUUsageId
        {
            get
            {
                return UnixSpecific.UnixTotalProcessorTime(process.Id);
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

        public bool WaitForProcess(int timeoutMilliseconds)
        {
            return process.WaitForExit(timeoutMilliseconds);
        }

        public int Dispose()
        {
			string paramz = Path.Combine(Locations.SupportScriptsLocation,"killtree.py") + " " +process.Id.ToString();
            if (!process.HasExited)
            {
                using (var killProcess = Process.Start("python", paramz))
                {
                    killProcess.Start();
                    killProcess.WaitForExit();
                    killProcess.Close();
                }
            }
            process.WaitForExit();
            var result = process.ExitCode;
            process.Close();
            return result;
        }

        public int Id
        {
            get { return process.Id; }
        }
    }

    public class ApplicationNotFoundException : Exception
    {
        public ApplicationNotFoundException(string application, Exception inner)
            : base(string.Format("Error launching application {0}", application), inner)
        {
        }
    }
}