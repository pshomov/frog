using System;
using SimpleCQRS;

namespace Frog.Domain
{
    public interface IValve
    {
        void Check(SourceRepoDriver repoUrl, string revision);
        event Action<string> OnUpdateFound;
        event Action<BuildStarted> OnBuildStarted;
        event Action<BuildUpdated> OnBuildUpdated;
        event Action<BuildEnded> OnBuildEnded;
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
                pipeline.Process(new SourceDrop(allocatedWorkingArea));
            }
        }

        public event Action<string> OnUpdateFound = s => {};
        public event Action<BuildStarted> OnBuildStarted;
        public event Action<BuildUpdated> OnBuildUpdated;
        public event Action<BuildEnded> OnBuildEnded;
    }

    public interface WorkingArea
    {
        string AllocateWorkingArea();
    }
}