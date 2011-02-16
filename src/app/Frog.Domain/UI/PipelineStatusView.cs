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
            report.PipelineStatus = new PipelineStatus(message.Status);
        }


        public void Handle(BuildEnded message)
        {
            report.Current = BuildStatus.Status.PipelineCompleted;
        }

        public class BuildStatus
        {
            public PipelineStatus PipelineStatus { get; set; }

            public enum Status { PipelineStarted, NotStarted, PipelineCompleted}
            public Status Current { get; set; }
        }

        public void Handle(BuildUpdated message)
        {
            report.Current = BuildStatus.Status.PipelineStarted;
            report.PipelineStatus = new PipelineStatus(message.Status);
        }
    }
}