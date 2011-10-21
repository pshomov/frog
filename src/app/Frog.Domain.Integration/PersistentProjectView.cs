using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using CorrugatedIron;
using CorrugatedIron.Exceptions;
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
            var connectionManager = Riak.GetConnectionManager(host, port).CreateClient();
            try
            {
                return connectionManager.Get<BuildStatus>(idsBucket, id.ToString());
            }
            catch (KeyNotFoundException)
            {
                throw new BuildNotFoundException();
            }
        }

        public Guid GetCurrentBuild(string repoUrl)
        {
            var connectionManager = Riak.GetConnectionManager(host, port).CreateClient();
            try
            {
                return connectionManager.Get<RepoInfo>(repoBucket, Riak.KeyGenerator(repoUrl)).CurrentBuild;
            }
            catch (KeyNotFoundException)
            {
                throw new RepositoryNotRegisteredException();
            }
        }

        public void SetCurrentBuild(string repoUrl, Guid buildId, string comment, string revision)
        {
            var connectionManager = Riak.GetConnectionManager(host, port).CreateClient();
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
            connectionManager.Put(new RiakObject(repoBucket, Riak.KeyGenerator(repoUrl), repoInfo));
            connectionManager.Put(new RiakObject(idsBucket, buildId.ToString(), new BuildStatus()));
        }

        public bool ProjectRegistered(string projectUrl)
        {
            var riakContentRepository = Riak.GetConnectionManager(host, port).CreateClient();
            try
            {
                riakContentRepository.Get<RepoInfo>(repoBucket, Riak.KeyGenerator(projectUrl));
                return true;
            }
            catch (KeyNotFoundException)
            {
                return false;
            }
        }

        public List<BuildHistoryItem> GetListOfBuilds(string repoUrl)
        {
            var connectionManager = Riak.GetConnectionManager(host, port).CreateClient();
            try
            {
                return connectionManager.Get<RepoInfo>(repoBucket, Riak.KeyGenerator(repoUrl)).BuildHistory;
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

            var client = Riak.GetConnectionManager(host, port).CreateClient();
            client.Put(new RiakObject(idsBucket, id.ToString(), a));
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
            var riak = Riak.GetConnectionManager(host, port);
            riak.CreateClient().Put(new RiakObject(idsBucket, id.ToString(), a));
        }

        public void BuildEnded(Guid id, BuildTotalEndStatus totalStatus)
        {
            var a = GetBuildStatus(id);
            a.BuildEnded(totalStatus);
            var riak = Riak.GetConnectionManager(host, port);
            riak.CreateClient().Put(new RiakObject(idsBucket, id.ToString(), a));
        }

        public void AppendTerminalOutput(Guid buildId, int taskIndex, int contentSequenceIndex, string content)
        {
            var a = GetBuildStatus(buildId);
            a.Tasks[taskIndex].AddTerminalOutput(contentSequenceIndex, content);
            var riak = Riak.GetConnectionManager(host, port);
            riak.CreateClient().Put(new RiakObject(idsBucket, buildId.ToString(), a));
        }

        private void WipeBuckett(IRiakCluster connectionManager, string bucket)
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