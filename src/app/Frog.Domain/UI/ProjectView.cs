using System;
using System.Collections.Concurrent;

namespace Frog.Domain.UI
{
    public class ProjectView
    {
        private readonly ConcurrentDictionary<Guid, BuildStatus> report;
        private readonly ConcurrentDictionary<string, Guid> currentBuild;

        public ProjectView(ConcurrentDictionary<Guid, BuildStatus> report, ConcurrentDictionary<string, Guid> currentBuild)
        {
            this.report = report;
            this.currentBuild = currentBuild;
        }

        public void SetBuildStatus(Guid id, BuildStatus value)
        {
            report[id] = value;
        }

        public BuildStatus GetBuildStatus(Guid id)
        {
            return report[id];
        }

        public Guid GetCurrentBuild(string repoUrl)
        {
            return currentBuild[repoUrl];
        }
    }
}