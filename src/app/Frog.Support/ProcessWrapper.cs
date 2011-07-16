using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Frog.Support
{
    public interface IProcessWrapper
    {
        TimeSpan TotalProcessorTime { get; }
        int ExitCode { get; }
        int Id { get; }
        event Action<string> OnErrorOutput;
        event Action<string> OnStdOutput;
        void Execute();
        bool WaitForProcess(int timeoutMilliseconds);
        void Kill();
        void MakeSureTerminalOutputIsFlushed();
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

        public bool HasExited
        {
            get { return process.HasExited; }
        }

        #region IProcessWrapper Members

        public event Action<string> OnErrorOutput = s => Console.WriteLine(String.Format("E>{0}", s));
        public event Action<string> OnStdOutput = s => Console.WriteLine(String.Format("S>{0}", s));

        public TimeSpan TotalProcessorTime
        {
            get
            {
                if (Os.IsUnix)
                {
                    return UnixSpecific.UnixTotalProcessorTime(process.Id);
                }
                else
                    return WindowsTotalProcessorTime();
            }
        }

        public int ExitCode
        {
            get { return process.ExitCode; }
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

        public void Kill()
        {
			string paramz = Path.Combine(Underware.SupportScriptsLocation,"killtree.py") + " " +process.Id.ToString();
            if (!process.HasExited)
            {
                using (var killProcess = Process.Start("python", paramz))
                {
                    killProcess.Start();
                    killProcess.WaitForExit();
                }
            }
        }

        public int Id
        {
            get { return process.Id; }
        }

        public void MakeSureTerminalOutputIsFlushed()
        {
            WaitForProcess();
        }

        #endregion

        private TimeSpan WindowsTotalProcessorTime()
        {
            Process[] processes = Process.GetProcesses();
            TimeSpan cputime = GetProcessCpuTime(processes, process);
            return cputime;
        }

        private TimeSpan GetProcessCpuTime(Process[] processes, Process process)
        {
            TimeSpan cputime = WinProcessProcessorTime(process);
            cputime += WinChildProcessesProcessorTime(processes, process);
            return cputime;
        }

        private TimeSpan WinChildProcessesProcessorTime(Process[] processes, Process process)
        {
            return
                TimeSpan.FromTicks(
                    processes.Where(p =>
                                        {
                                            try
                                            {
                                                Process parentProcess = ParentProcessUtilities.GetParentProcess(p.Handle);
                                                return (parentProcess != null) &&
                                                       (parentProcess.Handle == process.Handle);
                                            }
                                            catch (Win32Exception e)
                                            {
                                                if (e.NativeErrorCode == 5) return false;
                                                throw;
                                            }
                                        }).
                        Sum(childProcess => GetProcessCpuTime(processes, childProcess).Ticks));
        }

        private TimeSpan WinProcessProcessorTime(Process process)
        {
            return process.TotalProcessorTime;
        }

        public int WaitForProcess()
        {
            process.WaitForExit();
            MonoBugFix(process.ExitCode);
            return process.ExitCode;
        }

        private void MonoBugFix(int exitcode)
        {
            if (exitcode == 1) throw new ApplicationNotFoundException(cmdExe, null);
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