using System;
using Frog.Domain;
using SimpleCQRS;

namespace Frog.UI.Web
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
            report.Current = BuildStatus.Status.Started;
        }

        public void Handle(BuildEnded message)
        {
            report.Current = BuildStatus.Status.Complete;
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
            public enum Status { Started, NotStarted, Complete }
            public enum TaskExit { Dugh, Success, Error, Fail }
            public Status Current { get; set; }
            public TaskExit Completion { get; set; }
        }
    }
}