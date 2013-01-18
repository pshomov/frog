using System;
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

    public class RepositoryNotRegisteredException : Exception
    {
    }

    public class BuildNotFoundException : Exception
    {
    }
}