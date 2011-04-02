using System.Collections.Concurrent;
using Frog.Domain.UI;
using Frog.Specs.Support;

namespace Frog.Domain.Specs.PipelineStatusViewSpecs
{
    public abstract class StatusViewAfterBuildStarterSpecBase : BDD
    {
        protected PipelineStatusView View;
        protected PipelineStatus PipelineStatus;
        protected ConcurrentDictionary<string, BuildStatus> BuildStatuses;

        protected override void Given()
        {
            BuildStatuses = new ConcurrentDictionary<string, BuildStatus>();
            View = new PipelineStatusView(BuildStatuses);
        }
    }
}