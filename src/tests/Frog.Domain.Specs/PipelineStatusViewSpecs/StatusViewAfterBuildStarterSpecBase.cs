using System.Collections.Concurrent;
using Frog.Domain.UI;
using Frog.Specs.Support;

namespace Frog.Domain.Specs.PipelineStatusViewSpecs
{
    public abstract class StatusViewAfterBuildStarterSpecBase : BDD
    {
        protected UI.PipelineStatusView View;
        protected PipelineStatus PipelineStatus;
        protected ConcurrentDictionary<string, BuildStatuz> BuildStatuses;

        protected override void Given()
        {
            BuildStatuses = new ConcurrentDictionary<string, BuildStatuz>();
            View = new UI.PipelineStatusView(BuildStatuses);
        }
    }
}