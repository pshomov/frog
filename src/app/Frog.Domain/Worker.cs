using System;
using SimpleCQRS;

namespace Frog.Domain
{
    public class UpdateFound : Event
    {
        public string RepoUrl;
        public RevisionInfo Revision;
    }

    public class Worker
    {
        public virtual event Action<BuildTotalEndStatus> OnBuildEnded = status => { };
        public virtual event BuildStartedDelegate OnBuildStarted = status => { };
        public virtual event Action<int, Guid, TaskInfo.TaskStatus> OnBuildUpdated = (i, terminalId, status) => { };
        public virtual event ProjectCheckedOutDelegate OnProjectCheckedOut = info => { };
        public virtual event Action<TerminalUpdateInfo> OnTerminalUpdates = info => { };

        public Worker(Pipeline pipeline, WorkingAreaGoverner workingAreaGoverner)
        {
            this.pipeline = pipeline;
            this.workingAreaGoverner = workingAreaGoverner;
        }

        public virtual void ExecutePipelineForRevision(SourceRepoDriver repositoryDriver, string revision)
        {
            var allocatedWorkingArea = workingAreaGoverner.AllocateWorkingArea();
            var checkoutInfo = repositoryDriver.GetSourceRevision(revision, allocatedWorkingArea);
            OnProjectCheckedOut(checkoutInfo);
            ProcessPipeline(allocatedWorkingArea);
            workingAreaGoverner.DeallocateWorkingArea(allocatedWorkingArea);
        }

        readonly Pipeline pipeline;
        readonly WorkingAreaGoverner workingAreaGoverner;

        void ProcessPipeline(string allocatedWorkingArea)
        {
            pipeline.OnBuildEnded += OnBuildEnded;
            pipeline.OnBuildStarted += OnBuildStarted;
            pipeline.OnBuildUpdated += OnBuildUpdated;
            pipeline.OnTerminalUpdate += OnTerminalUpdates;
            try
            {
                pipeline.Process(new SourceDrop(allocatedWorkingArea));
            }
            finally
            {
                pipeline.OnBuildEnded -= OnBuildEnded;
                pipeline.OnBuildStarted -= OnBuildStarted;
                pipeline.OnBuildUpdated -= OnBuildUpdated;
                pipeline.OnTerminalUpdate -= OnTerminalUpdates;
            }
        }
    }
}