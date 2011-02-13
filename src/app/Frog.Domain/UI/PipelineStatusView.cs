using System;
using SimpleCQRS;

namespace Frog.Domain.UI
{
    public class PipelineStatusView : Handles<BuildStarted>, Handles<BuildEnded>
    {
        readonly BuildStatus report;

        public PipelineStatusView(BuildStatus report)
        {
            this.report = report;
        }

        public void Handle(BuildStarted message)
        {
            report.Current = BuildStatus.Status.PipelineStarted;
            report.Completion = BuildStatus.TaskExit.Dugh;
            report.TaskName = String.Empty;
        }

        public void Handle(TaskStarted message)
        {
            report.Current =
                BuildStatus.Status.TaskStarted;
            report.TaskName = message.Name;
        }

        public void Handle(BuildEnded message)
        {
            report.TaskName = "";
            report.Current = BuildStatus.Status.PipelineComplete;
            switch (message.Status)
            {
                case BuildEnded.BuildStatus.Error:
                    report.Completion = BuildStatus.TaskExit.Error;
                    break;
                case BuildEnded.BuildStatus.Success:
                    report.Completion = BuildStatus.TaskExit.Success;
                    break;
                default:
                    throw new ArgumentException("Build status argument not handled correctly, please report this error.");
            }
        }

        public class BuildStatus
        {
            public enum Status { PipelineStarted, NotStarted, PipelineComplete,
                TaskStarted,
                TaskEnded
            }
            public enum TaskExit { Dugh, Success, Error, Fail }
            public Status Current { get; set; }
            public TaskExit Completion { get; set; }
            public string TaskName { get; set; }
        }

        public void Handle(TaskFinished message)
        {
            report.TaskName = message.TaskName;
            report.Current = BuildStatus.Status.TaskEnded;
            switch (message.Status)
            {
                case ExecTaskResult.Status.Error:
                    report.Completion = BuildStatus.TaskExit.Error;
                    break;
                case ExecTaskResult.Status.Success:
                    report.Completion = BuildStatus.TaskExit.Success;
                    break;
                default:
                    throw new ArgumentException("Build status argument not handled correctly, please report this error.");
            }
        }
    }
}