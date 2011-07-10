using System;
using Frog.Support;

namespace Frog.Domain.ExecTasks
{
    public class ExecTaskResult
    {
        public enum Status
        {
            Success,
            Error
        } ;

        readonly ExecutionStatus executionStatus;
        readonly int exitCode;

        public ExecTaskResult(ExecutionStatus executionStatus, int exitCode)
        {
            this.executionStatus = executionStatus;
            this.exitCode = exitCode;
        }

        public int ExitCode
        {
            get
            {
                if (executionStatus != ExecutionStatus.Success)
                    throw new InvalidOperationException("Task did not execute, so there is no exit code");
                return exitCode;
            }
        }

        public bool HasExecuted
        {
            get { return executionStatus == ExecutionStatus.Success; }
        }

        public Status ExecStatus
        {
            get { return HasExecuted && ExitCode == 0 ? Status.Success : Status.Error; }
        }
    }

    public class ExecTask : IExecTask
    {
        readonly string app;
        readonly string arguments;
        readonly string name;
        private readonly Func<string, string, string, IProcessWrapper> processWrapperFactory;
        private readonly int periodLengthMs;
        private readonly int quotaNrPeriods;
        public event Action<int> OnTaskStarted = pid => {};
        public event Action<string> OnTerminalOutputUpdate = s => {};
        public ExecTask(string app, string arguments, string name, Func<string, string, string, IProcessWrapper> processWrapperFactory, int periodLengthMs = 5000, int quotaNrPeriods = 60)
        {
            this.app = app;
            this.arguments = arguments;
            this.name = name;
            this.processWrapperFactory = processWrapperFactory;
            this.periodLengthMs = periodLengthMs;
            this.quotaNrPeriods = quotaNrPeriods;
        }

        public ExecTaskResult Perform(SourceDrop sourceDrop)
        {
            IProcessWrapper process;
            try
            {
                process = processWrapperFactory(app, arguments, sourceDrop.SourceDropLocation);
                process.OnErrorOutput += s => { if (s != null) OnTerminalOutputUpdate("E>" + s + Environment.NewLine); };
                process.OnStdOutput += s => { if (s != null) OnTerminalOutputUpdate("S>" + s + Environment.NewLine); };
                OnTerminalOutputUpdate(string.Format("Runz>> Launching {0} with arguments {1} with currentFolder {2}",
                                                     app, arguments, sourceDrop.SourceDropLocation)+Environment.NewLine);
                process.Execute();
                OnTaskStarted(process.Id);
                ObserveProcess(process);
                process.Kill();
                var exitcode = process.WaitForProcess();
            }
            catch (ApplicationNotFoundException e)
            {
                OnTerminalOutputUpdate(string.Format("E>Process exited with error: {0}", e));
                return new ExecTaskResult(ExecutionStatus.Failure, -1);
            }

            if (process.HasExited)
            {
                OnTerminalOutputUpdate(string.Format("Runz>> Process has exited with exitcode {0}",
                                                     process.ExitCode) + Environment.NewLine);
                return new ExecTaskResult(ExecutionStatus.Success, process.ExitCode);
            }
            return new ExecTaskResult(ExecutionStatus.Failure, -1);
        }

        private void ObserveProcess(IProcessWrapper process)
        {
            var lastQuotaCPU = TimeSpan.FromTicks(0);
            for (int i = 0; i < quotaNrPeriods; i++)
            {
                if (process.WaitForProcess(periodLengthMs)) break;
                var currentQuotaCPU = process.TotalProcessorTime;
                if (currentQuotaCPU == lastQuotaCPU) break;
                lastQuotaCPU = currentQuotaCPU;
            }
        }

        public string Name
        {
            get { return name; }
        }
    }

    public enum ExecutionStatus
    {
        Success,
        Failure
    }
}