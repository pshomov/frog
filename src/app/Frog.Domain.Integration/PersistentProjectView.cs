using System;
using System.Collections.Generic;
using CorrugatedIron;
using CorrugatedIron.Models;
using Frog.Domain.UI;

namespace Frog.Domain.Integration
{

    public class PersistentProjectView : ProjectView
    {
        private readonly string host;
        private readonly int port;
        private readonly string idsBucket;
        private readonly string repoBucket;

        public PersistentProjectView(string host, int port, string IdsBucket, string repoBucket)
        {
            this.host = host;
            this.port = port;
            this.idsBucket = IdsBucket;
            this.repoBucket = repoBucket;
        }

        public BuildStatus GetBuildStatus(Guid id)
        {
            try
            {
                return Client.Get<BuildStatus>(idsBucket, id.ToString());
            }
            catch (KeyNotFoundException)
            {
                throw new BuildNotFoundException();
            }
        }

        private IRiakClient client;

        private IRiakClient Client
        {
            get { 
                return Riak.GetConnectionManager(host, port).CreateClient();
            }
        }

        public Guid GetCurrentBuild(string repoUrl)
        {
            try
            {
                return Client.Get<RepoInfo>(repoBucket, Riak.KeyGenerator(repoUrl)).CurrentBuild;
            }
            catch (KeyNotFoundException)
            {
                throw new RepositoryNotRegisteredException();
            }
        }

        public void SetCurrentBuild(string repoUrl, Guid buildId, string comment, string revision)
        {
            var connectionManager = Client;
            var repoInfo = new RepoInfo();
            try
            {
                repoInfo = connectionManager.Get<RepoInfo>(repoBucket, Riak.KeyGenerator(repoUrl));
            }
            catch (KeyNotFoundException)
            {
            }
            repoInfo.BuildHistory.Add(new BuildHistoryItem(){BuildId = buildId, Comment = comment, Revision = revision});
            repoInfo.CurrentBuild = buildId;
            connectionManager.Put(repoBucket, Riak.KeyGenerator(repoUrl), repoInfo);
            connectionManager.Put(idsBucket, buildId.ToString(), new BuildStatus());
        }

        public bool ProjectRegistered(string projectUrl)
        {
            try
            {
                Client.Get<RepoInfo>(repoBucket, Riak.KeyGenerator(projectUrl));
                return true;
            }
            catch (KeyNotFoundException)
            {
                return false;
            }
        }

        public List<BuildHistoryItem> GetListOfBuilds(string repoUrl)
        {
            try
            {
                return Client.Get<RepoInfo>(repoBucket, Riak.KeyGenerator(repoUrl)).BuildHistory;
            }
            catch (KeyNotFoundException)
            {
                return new List<BuildHistoryItem>();
            }
        }

        public void SetBuildStarted(Guid id, IEnumerable<TaskInfo> taskInfos)
        {
            var a = GetBuildStatus(id);
            a.BuildStarted(taskInfos);

            Client.Put(idsBucket, id.ToString(), a);
        }

        public void WipeBucket()
        {
            var connectionManager = Riak.GetConnectionManager(host, port);
            WipeBuckett(connectionManager, idsBucket);
            WipeBuckett(connectionManager, repoBucket);
        }

        public void BuildUpdated(Guid id, int taskIndex, TaskInfo.TaskStatus taskStatus)
        {
            var a = GetBuildStatus(id);
            a.BuildUpdated(taskIndex, taskStatus);
            Client.Put(idsBucket, id.ToString(), a);
        }

        public void BuildEnded(Guid id, BuildTotalEndStatus totalStatus)
        {
            var a = GetBuildStatus(id);
            a.BuildEnded(totalStatus);
            Client.Put(idsBucket, id.ToString(), a);
        }

        public void AppendTerminalOutput(Guid buildId, int taskIndex, int contentSequenceIndex, string content)
        {
            var a = GetBuildStatus(buildId);
            a.Tasks[taskIndex].AddTerminalOutput(contentSequenceIndex, content);
            Client.Put(idsBucket, buildId.ToString(), a);

        }

        private void WipeBuckett(IRiakEndPoint connectionManager, string bucket)
        {
            var client = connectionManager.CreateClient();
            client.DeleteBucket(bucket);
        }
    }

    public class BuildInfo
    {
        public Guid id;
        public string comment;
    }

    public class RepoInfo
    {
        public Guid CurrentBuild;
        public List<BuildHistoryItem> BuildHistory = new List<BuildHistoryItem>();

    }
}