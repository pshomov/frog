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
        }

        public void Handle(TaskStarted message)
        {
            report.Current =
                BuildStatus.Status.TaskStarted;
        }

        public void Handle(BuildEnded message)
        {
            report.Current = BuildStatus.Status.PipelineComplete;
            switch (message.Status)
            {
                case BuildEnded.BuildStatus.Fail:
                    report.Completion = BuildStatus.TaskExit.Fail;
                    break;
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
                TaskStarted
            }
            public enum TaskExit { Dugh, Success, Error, Fail }
            public Status Current { get; set; }
            public TaskExit Completion { get; set; }
        }
    }
}