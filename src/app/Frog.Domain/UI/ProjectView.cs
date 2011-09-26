using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Frog.Domain.UI
{
    public struct BuildHistoryItem
    {
        public Guid BuildId;
        public string Comment;
        public string Revision;
    }
    public class ProjectView
    {
        private readonly ConcurrentDictionary<Guid, BuildStatus> report;
        private readonly ConcurrentDictionary<string, Guid> currentBuild;
        private readonly ConcurrentDictionary<string, List<BuildHistoryItem>> buildHistory;

        public ProjectView()
        {
            report = new ConcurrentDictionary<Guid, BuildStatus>();
            currentBuild = new ConcurrentDictionary<string, Guid>();
            buildHistory = new ConcurrentDictionary<string, List<BuildHistoryItem>>();
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

        public void SetCurrentBuild(string repoUrl, Guid buildId, string comment, string revision)
        {
            currentBuild[repoUrl] = buildId;
            buildHistory.TryAdd(repoUrl, new List<BuildHistoryItem>());
            buildHistory[repoUrl].Add(new BuildHistoryItem(){BuildId = buildId, Comment = comment, Revision = revision});
        }

        public bool ProjectRegistered(string projectUrl)
        {
            return currentBuild.ContainsKey(projectUrl);
        }

        public List<BuildHistoryItem> GetListOfBuilds(string repoUrl)
        {
            return buildHistory[repoUrl];
        }
    }
}