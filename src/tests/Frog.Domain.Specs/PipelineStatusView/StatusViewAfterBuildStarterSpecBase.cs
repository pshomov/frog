using System.Collections.Concurrent;
using Frog.Specs.Support;

namespace Frog.Domain.Specs.PipelineStatusView
{
    public abstract class StatusViewAfterBuildStarterSpecBase : BDD
    {
        protected UI.PipelineStatusView view;
        protected PipelineStatus pipelineStatus;
        protected ConcurrentDictionary<string, UI.PipelineStatusView.BuildStatus> buildStatuses;

        protected override void Given()
        {
            buildStatuses = new ConcurrentDictionary<string, UI.PipelineStatusView.BuildStatus>();
            view = new UI.PipelineStatusView(buildStatuses);
        }
    }
}