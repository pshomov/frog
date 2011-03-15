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
        readonly WorkingArea workingArea;

        public Worker(Pipeline pipeline, WorkingArea workingArea)
        {
            this.pipeline = pipeline;
            this.workingArea = workingArea;
        }

        public virtual void CheckForUpdatesAndKickOffPipeline(SourceRepoDriver repo, string revision)
        {
            string latestRev;
            if ((latestRev = repo.GetLatestRevision()) != revision)
            {
                OnUpdateFound(latestRev);
                var allocatedWorkingArea = workingArea.AllocateWorkingArea();
                repo.GetSourceRevision(latestRev, allocatedWorkingArea);
                ProcessPipeline(allocatedWorkingArea);
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

    public interface WorkingArea
    {
        string AllocateWorkingArea();
    }
}