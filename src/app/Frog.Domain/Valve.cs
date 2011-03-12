using System;
using SimpleCQRS;

namespace Frog.Domain
{
    public interface IValve
    {
        void Check(SourceRepoDriver repoUrl, string revision);
        event Action<string> OnUpdateFound;
        event Action<PipelineStatus> OnBuildStarted;
        event Action<PipelineStatus> OnBuildUpdated;
        event Action<BuildTotalStatus> OnBuildEnded;
    }

    public class UpdateFound : Event
    {
        public string Revision;
        public string RepoUrl;
    }

    public class Valve : IValve
    {
        readonly Pipeline pipeline;
        readonly WorkingArea workingArea;

        public Valve(Pipeline pipeline, WorkingArea workingArea)
        {
            this.pipeline = pipeline;
            this.workingArea = workingArea;
        }

        public void Check(SourceRepoDriver repoUrl, string revision)
        {
            string latestRev;
            if ((latestRev = repoUrl.GetLatestRevision()) != revision)
            {
                OnUpdateFound(latestRev);
                var allocatedWorkingArea = workingArea.AllocateWorkingArea();
                repoUrl.GetSourceRevision(latestRev, allocatedWorkingArea);
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

        public event Action<string> OnUpdateFound = s => {};
        public event Action<PipelineStatus> OnBuildStarted;
        public event Action<PipelineStatus> OnBuildUpdated;
        public event Action<BuildTotalStatus> OnBuildEnded;
    }

    public interface WorkingArea
    {
        string AllocateWorkingArea();
    }
}