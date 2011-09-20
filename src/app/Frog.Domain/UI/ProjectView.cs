using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Frog.Domain.UI
{
    public class ProjectView
    {
        private readonly ConcurrentDictionary<Guid, BuildStatus> report;
        private readonly ConcurrentDictionary<string, Guid> currentBuild;
        private readonly ConcurrentDictionary<string, List<Guid>> buildHistory;

        public ProjectView()
        {
            report = new ConcurrentDictionary<Guid, BuildStatus>();
            currentBuild = new ConcurrentDictionary<string, Guid>();
            buildHistory = new ConcurrentDictionary<string, List<Guid>>();
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
            buildHistory.TryAdd(repoUrl, new List<Guid>());
            buildHistory[repoUrl].Add(buildId);
        }

        public bool ProjectRegistered(string projectUrl)
        {
            return currentBuild.ContainsKey(projectUrl);
        }

        public List<Guid> GetListOfBuilds(string repoUrl)
        {
            return buildHistory[repoUrl];
        }
    }
}