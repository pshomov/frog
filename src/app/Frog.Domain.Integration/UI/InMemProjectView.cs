using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Frog.Domain.Integration.UI
{
    public struct BuildHistoryItem
    {
        public Guid BuildId;
        public string Comment;
        public string Revision;
    }

    public interface ProjectTestSupport
    {
        void WipeBucket();
    }

    public interface BuildView
    {
        void BuildEnded(Guid id, BuildTotalEndStatus totalStatus);
        void BuildUpdated(Guid id, int taskIndex, TaskInfo.TaskStatus taskStatus);
        BuildStatus GetBuildStatus(Guid id);
        void SetBuildStarted(Guid id, IEnumerable<TaskInfo> taskInfos);
    }

    public interface ProjectView
    {
        Guid GetCurrentBuild(string repoUrl);
        List<BuildHistoryItem> GetListOfBuilds(string repoUrl);
        bool IsProjectRegistered(string projectUrl);
        void SetCurrentBuild(string repoUrl, Guid buildId, string comment, string revision);
    }

    public class InMemProjectView : ProjectView, BuildView, ProjectTestSupport
    {
        public InMemProjectView()
        {
            report = new ConcurrentDictionary<Guid, BuildStatus>();
            currentBuild = new ConcurrentDictionary<string, Guid>();
            buildHistory = new ConcurrentDictionary<string, List<BuildHistoryItem>>();
        }

        public void BuildEnded(Guid id, BuildTotalEndStatus totalStatus)
        {
            GetBuildStatus(id).BuildEnded(totalStatus);
        }

        public void BuildUpdated(Guid id, int taskIndex, TaskInfo.TaskStatus taskStatus)
        {
            GetBuildStatus(id).BuildUpdated(taskIndex, taskStatus);
        }

        public BuildStatus GetBuildStatus(Guid id)
        {
            try
            {
                return report[id];
            }
            catch (System.Collections.Generic.KeyNotFoundException)
            {
                throw new BuildNotFoundException();
            }
        }

        public Guid GetCurrentBuild(string repoUrl)
        {
            try
            {
                return currentBuild[repoUrl];
            }
            catch (System.Collections.Generic.KeyNotFoundException)
            {
                throw new RepositoryNotRegisteredException();
            }
        }

        public List<BuildHistoryItem> GetListOfBuilds(string repoUrl)
        {
            if (!buildHistory.ContainsKey(repoUrl)) return new List<BuildHistoryItem>();
            return buildHistory[repoUrl];
        }

        public bool IsProjectRegistered(string projectUrl)
        {
            return currentBuild.ContainsKey(projectUrl);
        }

        public void SetBuildStarted(Guid id, IEnumerable<TaskInfo> taskInfos)
        {
            GetBuildStatus(id).BuildStarted(taskInfos);
        }

        public void SetCurrentBuild(string repoUrl, Guid buildId, string comment, string revision)
        {
            currentBuild[repoUrl] = buildId;
            buildHistory.TryAdd(repoUrl, new List<BuildHistoryItem>());
            buildHistory[repoUrl].Add(new BuildHistoryItem {BuildId = buildId, Comment = comment, Revision = revision});
            report[buildId] = new BuildStatus();
        }

        public void WipeBucket()
        {
        }

        readonly ConcurrentDictionary<Guid, BuildStatus> report;
        readonly ConcurrentDictionary<string, Guid> currentBuild;
        readonly ConcurrentDictionary<string, List<BuildHistoryItem>> buildHistory;
    }

    public class RepositoryNotRegisteredException : Exception
    {
    }

    public class BuildNotFoundException : Exception
    {
    }
}