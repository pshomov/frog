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
        private IProcessWrapper process;
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
            process = processWrapperFactory(app, arguments, sourceDrop.SourceDropLocation);
            process.OnErrorOutput += s => { if (s != null) OnTerminalOutputUpdate("E>" + s + "\r\n"); };
            process.OnStdOutput += s => { if (s != null) OnTerminalOutputUpdate("S>" + s + "\r\n"); };
            OnTerminalOutputUpdate(string.Format("Runz>> Launching {0} with arguments {1} with currentFolder {2}",
                                                 app, arguments, sourceDrop.SourceDropLocation) + "\r\n");
            try
            {
                process.Execute();
                OnTaskStarted(process.Id);
                ObserveTask();
                process.MakeSureTerminalOutputIsFlushed();
            }
            catch(HangingProcessDetectedException)
            {
                process.Kill();
                process.MakeSureTerminalOutputIsFlushed();
                OnTerminalOutputUpdate(string.Format("Runz>> It looks like task is hanging without doing much. Task was killed. Exit code: {0}",
                                                        process.ExitCode) + "\r\n");
                return new ExecTaskResult(ExecutionStatus.Failure, process.ExitCode);
            }
            catch(TaskQuotaConsumedException)
            {
                process.Kill();
                process.MakeSureTerminalOutputIsFlushed();
                OnTerminalOutputUpdate(string.Format("Runz>> Task has consumed all its quota. Task was killed. Exit code: {0}",
                                                     process.ExitCode) + "\r\n");
                return new ExecTaskResult(ExecutionStatus.Failure, process.ExitCode);
            }
            catch (ApplicationNotFoundException e)
            {
                OnTerminalOutputUpdate(string.Format("E> Task has exited with error: {0}", e));
                return new ExecTaskResult(ExecutionStatus.Failure, -1);
            }

            OnTerminalOutputUpdate(string.Format("Runz>> Task has exited with exitcode {0}",
                                                    process.ExitCode) + "\r\n");
            return new ExecTaskResult(ExecutionStatus.Success, process.ExitCode);
        }

        private void ObserveTask()
        {
            var lastQuotaCPU = TimeSpan.FromTicks(0);
            for (int i = 0; i < quotaNrPeriods; i++)
            {
                if (process.WaitForProcess(periodLengthMs)) return;
                var currentQuotaCPU = process.TotalProcessorTime;
                if (HangingTaskDetector(lastQuotaCPU, currentQuotaCPU)) throw new HangingProcessDetectedException();
                lastQuotaCPU = currentQuotaCPU;
            }
            throw new TaskQuotaConsumedException();
        }

        private static bool HangingTaskDetector(TimeSpan lastQuotaCPU, TimeSpan currentQuotaCPU)
        {
            return currentQuotaCPU == lastQuotaCPU;
        }

        public string Name
        {
            get { return name; }
        }
    }

    internal class HangingProcessDetectedException : Exception
    {
    }

    internal class TaskQuotaConsumedException : Exception
    {
    }

    public enum ExecutionStatus
    {
        Success,
        Failure
    }
}