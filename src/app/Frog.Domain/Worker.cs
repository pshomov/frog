using System;
using Frog.Support;
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

        public virtual void CheckForUpdatesAndKickOffPipeline(SourceRepoDriver repo, string revision)
        {
            string latestRev;
            if ((latestRev = repo.GetLatestRevision()) != revision)
            {
                OnUpdateFound(latestRev);
                var allocatedWorkingArea = workingAreaGoverner.AllocateWorkingArea();
                repo.GetSourceRevision(latestRev, allocatedWorkingArea);
                ProcessPipeline(allocatedWorkingArea);
                workingAreaGoverner.DeallocateWorkingArea(allocatedWorkingArea);
            }
        }

        void ProcessPipeline(string allocatedWorkingArea)
        {
            pipeline.OnBuildEnded += OnBuildEnded;
            pipeline.OnBuildStarted += OnBuildStarted;
            pipeline.OnBuildUpdated += OnBuildUpdated;
            pipeline.Process(new SourceDrop(allocatedWorkingArea));
            pipeline.OnBuildEnded -= OnBuildEnded;
            pipeline.OnBuildStarted -= OnBuildStarted;
            pipeline.OnBuildUpdated -= OnBuildUpdated;
        }

        public virtual event Action<string> OnUpdateFound = s => {};
        public virtual event Action<PipelineStatus> OnBuildStarted;
        public virtual event Action<PipelineStatus> OnBuildUpdated;
        public virtual event Action<BuildTotalStatus> OnBuildEnded;
    }

    public interface WorkingAreaGoverner
    {
        string AllocateWorkingArea();
        void DeallocateWorkingArea(string allocatedArea);
    }
}