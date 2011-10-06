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

    public interface ProjectView
    {
        BuildStatus GetBuildStatus(Guid id);
        Guid GetCurrentBuild(string repoUrl);
        void SetCurrentBuild(string repoUrl, Guid buildId, string comment, string revision);
        bool ProjectRegistered(string projectUrl);
        List<BuildHistoryItem> GetListOfBuilds(string repoUrl);
        void SetBuildStarted(Guid id, IEnumerable<TaskInfo> taskInfos);
        void WipeBucket();
        void BuildUpdated(Guid id, int taskIndex, TaskInfo.TaskStatus taskStatus);
        void BuildEnded(Guid id, BuildTotalEndStatus totalStatus);
        void AppendTerminalOutput(Guid buildId, int taskIndex, int contentSequenceIndex, string content);
    }

    public class InMemProjectView : ProjectView
    {
        private readonly ConcurrentDictionary<Guid, BuildStatus> report;
        private readonly ConcurrentDictionary<string, Guid> currentBuild;
        private readonly ConcurrentDictionary<string, List<BuildHistoryItem>> buildHistory;

        public InMemProjectView()
        {
            report = new ConcurrentDictionary<Guid, BuildStatus>();
            currentBuild = new ConcurrentDictionary<string, Guid>();
            buildHistory = new ConcurrentDictionary<string, List<BuildHistoryItem>>();
        }

        public BuildStatus GetBuildStatus(Guid id)
        {
            report.TryAdd(id, new BuildStatus());
            return report[id];
        }

        public Guid GetCurrentBuild(string repoUrl)
        {
            try
            {
                return currentBuild[repoUrl];
            }
            catch (KeyNotFoundException e)
            {
                throw new RepositoryNotRegisteredException();
            }
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
            if (!buildHistory.ContainsKey(repoUrl)) return new List<BuildHistoryItem>();
            return buildHistory[repoUrl];
        }

        public void SetBuildStarted(Guid id, IEnumerable<TaskInfo> taskInfos)
        {
            GetBuildStatus(id).BuildStarted(taskInfos);
        }

        public void WipeBucket()
        {
            
        }

        public void BuildUpdated(Guid id, int taskIndex, TaskInfo.TaskStatus taskStatus)
        {
            GetBuildStatus(id).BuildUpdated(taskIndex, taskStatus);
        }

        public void BuildEnded(Guid id, BuildTotalEndStatus totalStatus)
        {
            GetBuildStatus(id).BuildEnded(totalStatus);
        }

        public void AppendTerminalOutput(Guid buildId, int taskIndex, int contentSequenceIndex, string content)
        {
            GetBuildStatus(buildId).Tasks[taskIndex].AddTerminalOutput(contentSequenceIndex,
                                                                                         content);
        }
    }

    public class RepositoryNotRegisteredException : Exception
    {
    }

    public class BuildNotFoundException : Exception
    {
    }

}