using System;
using System.Collections.Concurrent;
using Frog.Domain.RepositoryTracker;
using Frog.Domain.UI;
using Frog.Specs.Support;
using Frog.Support;

namespace Frog.Domain.Specs.PipelineStatusViewSpecs
{
    public abstract class StatusViewAfterBuildStarterSpecBase : BDD
    {
        protected PipelineStatusView View;
        protected PipelineStatus PipelineStatus;
        protected ConcurrentDictionary<Guid, BuildStatus> BuildStatuses;
        protected BuildStarted BuildMessage;
        private ConcurrentDictionary<string, Guid> currentBuilds;

        protected override void Given()
        {
            BuildStatuses = new ConcurrentDictionary<Guid, BuildStatus>();
            currentBuilds = new ConcurrentDictionary<string, Guid>();
            View = new PipelineStatusView(BuildStatuses, currentBuilds);

            BuildMessage = new BuildStarted{RepoUrl = "http://somecoolproject", BuildId = Guid.NewGuid(), Status = new PipelineStatus(Guid.NewGuid())
                                 {
                                     Tasks =
                                         {
                                             new TaskInfo
                                                 {Name = "task1", Status = TaskInfo.TaskStatus.NotStarted}
                                         }
                                 }};

            View.Handle(BuildMessage);
        }
    }
}