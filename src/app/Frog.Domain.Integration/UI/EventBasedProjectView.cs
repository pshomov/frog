using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using EventStore;
using SimpleCQRS;

namespace Frog.Domain.Integration.UI
{
//    public class BuildA : AggregateRoot
//    {
//        public BuildStatus BuildStatus
//        {
//            get { return buildStatus; }
//        }
//
//        public override Guid Id
//        {
//            get { return id; }
//        }
//
//        public BuildA(Guid id)
//        {
//            this.id = id;
//        }
//
//        readonly Guid id;
//        readonly BuildStatus buildStatus = new BuildStatus();
//
//        void Apply(ProjectCheckedOut msg)
//        {
//            return;
//        }
//
//        void Apply(BuildStarted msg)
//        {
//            BuildStatus.BuildStarted(msg.Status.Tasks);
//        }
//
//        void Apply(BuildUpdated msg)
//        {
//            BuildStatus.BuildUpdated(msg.TaskIndex, msg.TaskStatus);
//        }
//
//        void Apply(BuildEnded msg)
//        {
//            BuildStatus.BuildEnded(msg.TotalStatus);
//        }
//    }
//
//    public class EventBasedProjectView : ProjectView, BuildView, ProjectTestSupport
//    {
//        public EventBasedProjectView(IStoreEvents eventStore)
//        {
//            this.eventStore = eventStore;
//        }
//
//        public void AppendTerminalOutput(Guid buildId, int taskIndex, int contentSequenceIndex, string content)
//        {
//            throw new NotImplementedException();
//        }
//
//        public void BuildEnded(Guid id, BuildTotalEndStatus totalStatus)
//        {
//            throw new NotImplementedException();
//        }
//
//        public void BuildUpdated(Guid id, int taskIndex, TaskInfo.TaskStatus taskStatus)
//        {
//            throw new NotImplementedException();
//        }
//
//        public BuildStatus GetBuildStatus(Guid id)
//        {
//            var commits = eventStore.Advanced.GetFrom(id, Int32.MinValue, Int32.MaxValue);
//            var build = new BuildA(id);
//            build.LoadsFromHistory(commits.Select(commit => (Event) commit.Events[0].Body));
//            return build.BuildStatus;
//        }
//
//        public Guid GetCurrentBuild(string repoUrl)
//        {
//            var id = KeyGenerator(repoUrl);
//            var commits = eventStore.Advanced.GetFrom(id, Int32.MinValue, Int32.MaxValue);
//            var project = new Project(id);
//            project.LoadsFromHistory(commits.Select(commit => (Event) commit.Events[0].Body));
//            return project.CurrentBuildId;
//        }
//
//        public List<BuildHistoryItem> GetListOfBuilds(string repoUrl)
//        {
//            var id = KeyGenerator(repoUrl);
//            var commits = eventStore.Advanced.GetFrom(id, Int32.MinValue, Int32.MaxValue);
//            var project = new Project(id);
//            project.LoadsFromHistory(commits.Select(commit => (Event) commit.Events[0].Body));
//            return project.Builds;
//        }
//
//        public bool IsProjectRegistered(string projectUrl)
//        {
//            var id = KeyGenerator(projectUrl);
//            var commits = eventStore.Advanced.GetFrom(id, Int32.MinValue, Int32.MaxValue);
//            return commits.Any();
//        }
//
//        public static Guid KeyGenerator(string repoUrl)
//        {
//            var hash = new MD5CryptoServiceProvider().ComputeHash(Encoding.UTF8.GetBytes(repoUrl));
//            return new Guid(hash);
//        }
//
//        public void SetBuildStarted(Guid id, IEnumerable<TaskInfo> taskInfos)
//        {
//            throw new NotImplementedException();
//        }
//
//        public void SetCurrentBuild(string repoUrl, Guid buildId, string comment, string revision)
//        {
//            throw new NotImplementedException();
//        }
//
//        public void WipeBucket()
//        {
//            eventStore.Advanced.Purge();
//            eventStore.Advanced.Initialize();
//        }
//
//        readonly IStoreEvents eventStore;
//    }
//
//    public class Project : AggregateRoot
//    {
//        public List<BuildHistoryItem> Builds
//        {
//            get { return builds; }
//            set { builds = value; }
//        }
//
//        public Guid CurrentBuildId { get; private set; }
//
//        public override Guid Id
//        {
//            get { return id; }
//        }
//
//        public Project(Guid id)
//        {
//            this.id = id;
//        }
//
//        readonly Guid id;
//        List<BuildHistoryItem> builds = new List<BuildHistoryItem>();
//
//        void Apply(ProjectCheckedOut msg)
//        {
//            CurrentBuildId = msg.BuildId;
//            builds.Add(new BuildHistoryItem {BuildId = msg.BuildId, Comment = msg.CheckoutInfo.Comment, Revision = msg.CheckoutInfo.Revision});
//        }
//    }
}