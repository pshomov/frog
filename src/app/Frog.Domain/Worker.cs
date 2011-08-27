using System;
using SimpleCQRS;

namespace Frog.Domain
{
    public class UpdateFound : Event
    {
        public string Revision;
        public string RepoUrl;
    }

    public class Worker
    {
        readonly Pipeline pipeline;
        readonly WorkingAreaGoverner workingAreaGoverner;

        public Worker(Pipeline pipeline, WorkingAreaGoverner workingAreaGoverner)
        {
            this.pipeline = pipeline;
            this.workingAreaGoverner = workingAreaGoverner;
        }

        public virtual void CheckForUpdatesAndKickOffPipeline(SourceRepoDriver repositoryDriver, string revision)
        {
            string latestRevision = repositoryDriver.GetLatestRevision();
            OnUpdateFound(latestRevision);
            if (latestRevision != revision)
            {
                var allocatedWorkingArea = workingAreaGoverner.AllocateWorkingArea();
                repositoryDriver.GetSourceRevision(latestRevision, allocatedWorkingArea);
                ProcessPipeline(allocatedWorkingArea);
                workingAreaGoverner.DeallocateWorkingArea(allocatedWorkingArea);
            }
        }

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

        public virtual event Action<string> OnUpdateFound = s => {};
        public virtual event Action<TerminalUpdateInfo> OnTerminalUpdates = info => {};
        public virtual event BuildStartedDelegate OnBuildStarted = status => {};
        public virtual event Action<int, TaskInfo.TaskStatus> OnBuildUpdated =  (i,status) => {};
        public virtual event Action<BuildTotalEndStatus> OnBuildEnded = status => {};
    }
}