using System;
using System.Diagnostics;

namespace Frog.Support
{
    public class ProcessWrapper
    {
        public delegate void OutputCallback(string output);
        readonly string cmdExe;
        readonly string arguments;
        Process process;
        public event OutputCallback OnErrorOutput; 
        public ProcessWrapper(string cmdExe, string arguments)
        {
            this.cmdExe = cmdExe;
            this.arguments = arguments;
        }

        public void Execute()
        {
            process = ProcessHelpers.Start(cmdExe, arguments);
            process.ErrorDataReceived += (sender, args) => OnErrorOutput(args.Data);
            process.OutputDataReceived += (sender, args) => OnStdOutput(args.Data);
            process.BeginErrorReadLine();
            process.BeginOutputReadLine();
        }

        public void WaitForProcess()
        {
            process.WaitForExit();
        }

        public event Action<String> OnStdOutput;
    }
}