using System.Collections.Concurrent;
using Frog.Specs.Support;

namespace Frog.Domain.Specs.PipelineStatusView
{
    public abstract class StatusViewAfterBuildStarterSpecBase : BDD
    {
        protected UI.PipelineStatusView View;
        protected PipelineStatus PipelineStatus;
        protected ConcurrentDictionary<string, UI.PipelineStatusView.BuildStatus> BuildStatuses;

        protected override void Given()
        {
            BuildStatuses = new ConcurrentDictionary<string, UI.PipelineStatusView.BuildStatus>();
            View = new UI.PipelineStatusView(BuildStatuses);
        }
    }
}