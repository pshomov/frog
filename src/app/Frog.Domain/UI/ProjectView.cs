using System;
using System.Collections.Concurrent;

namespace Frog.Domain.UI
{
    public class ProjectView
    {
        private readonly ConcurrentDictionary<Guid, BuildStatus> report;
        private readonly ConcurrentDictionary<string, Guid> currentBuild;

        public ProjectView()
        {
            report = new ConcurrentDictionary<Guid, BuildStatus>();
            currentBuild = new ConcurrentDictionary<string, Guid>();
        }

        public void SetBuildStatus(Guid id, BuildStatus value)
        {
            report[id] = value;
        }

        public BuildStatus GetBuildStatus(Guid id)
        {
            report.TryAdd(id, new BuildStatus());
            return report[id];
        }

        public Guid GetCurrentBuild(string repoUrl)
        {
            return currentBuild[repoUrl];
        }

        public void SetCurrentBuild(string repoUrl, Guid buildId)
        {
            currentBuild[repoUrl] = buildId;
        }
    }
}