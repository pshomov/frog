using System;
using System.Collections.Generic;
using System.Data.RiakClient;
using System.Data.RiakClient.Models;
using System.IO;
using System.Linq;
using System.Web.Script.Serialization;
using CorrugatedIron;
using CorrugatedIron.Comms;
using CorrugatedIron.Config;
using CorrugatedIron.Models.MapReduce;
using Frog.Domain.UI;
using riak.net.Models;
using riak.net.ProtoModels;

namespace Frog.Domain.Integration
{
    public class PersistentProjectView : ProjectView
    {
        private readonly string host;
        private readonly int port;
        private readonly string idsBucket;
        private readonly string repoBucket;
        readonly JavaScriptSerializer jsonBridge = new JavaScriptSerializer();

        public PersistentProjectView(string host, int port, string IdsBucket, string repoBucket)
        {
            this.host = host;
            this.port = port;
            this.idsBucket = IdsBucket;
            this.repoBucket = repoBucket;
        }

        public BuildStatus GetBuildStatus(Guid id)
        {
            var connectionManager = GetConnectionManager();
            var riakContentRepository = new RiakContentRepository(connectionManager);
            try
            {
                var riakResponse =
                    riakContentRepository.Find(new RiakFindRequest { Bucket = idsBucket, Keys =  new []{id.ToString()}, ReadValue = 1 });
                return riakResponse.Result.Select(document => jsonBridge.Deserialize<BuildStatus>(document.Value)).Single();
            }
            catch (InvalidOperationException e)
            {
                throw new BuildNotFoundException();
            }
        }

        public Guid GetCurrentBuild(string repoUrl)
        {
            var connectionManager = GetConnectionManager();
            var riakContentRepository = new RiakContentRepository(connectionManager);
            try
            {
                var riakResponse =
                    riakContentRepository.Find(new RiakFindRequest { Bucket = repoBucket, Keys = new[] {repoUrl}, ReadValue = 1 });
                return riakResponse.Result.Select(document => jsonBridge.Deserialize<RepoInfo>(document.Value)).Single().CurrentBuild;
            }
            catch (InvalidOperationException e)
            {
                throw new RepositoryNotRegisteredException();
            }
        }

        public void SetCurrentBuild(string repoUrl, Guid buildId, string comment, string revision)
        {
            RiakConnectionManager connectionManager = GetConnectionManager();
            var riakConnection = new RiakContentRepository(connectionManager);
            RepoInfo repoInfo = new RepoInfo();
            try
            {
                var riakResponse =
                    riakConnection.Find(new RiakFindRequest { Bucket = repoBucket, Keys = new[] { repoUrl }, ReadValue = 1 });
                if (riakResponse.Result.Count() == 1) repoInfo = riakResponse.Result.Select(document => jsonBridge.Deserialize<RepoInfo>(document.Value)).Single();
            }
            catch (InvalidOperationException e)
            {
                
            }
            repoInfo.BuildHistory.Add(buildId);
            repoInfo.CurrentBuild = buildId;
            riakConnection.Persist(new RiakPersistRequest { Bucket = repoBucket, Key = repoUrl, Content = new RiakContent { Value = jsonBridge.Serialize(repoInfo).GetBytes() } });
            riakConnection.Persist(new RiakPersistRequest { Bucket = idsBucket, Key = buildId.ToString(), Content = new RiakContent { Value = jsonBridge.Serialize(new BuildStatus()).GetBytes()}});
        }

        public bool ProjectRegistered(string projectUrl)
        {
            var connectionManager = GetConnectionManager();
            var riakContentRepository = new RiakContentRepository(connectionManager);
            try
            {
                var riakResponse =
                    riakContentRepository.Find(new RiakFindRequest { Bucket = repoBucket, Keys = new[] { projectUrl }, ReadValue = 1 });
                return riakResponse.Result.Count() == 1;
            }
            catch (InvalidOperationException e)
            {
                return false;
            }
        }

        public List<BuildHistoryItem> GetListOfBuilds(string repoUrl)
        {
//            var cl = RiakCluster.FromConfig("riakConfig", Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "riak.config"));
//            var client = cl.CreateClient();
//            var result = client.Ping();
//            if (!result.IsSuccess) throw new ApplicationException("can't connect to database");
//            var q =
//                new RiakMapReduceQuery().Inputs(idsBucket).MapJs(
//                    javascript =>
//                    javascript.Source(
//                        @"function(value, key,a){var v = Riak.MapValuesJson(value); if (v.repoUrl === a) return key;}").Argument(repoUrl));
//            var riakResult = client.MapReduce(q);
            var connectionManager = GetConnectionManager();
            var riakConnection = new RiakContentRepository(connectionManager);
            var repoInfo = new RepoInfo();
            try
            {
                var riakResponse =
                    riakConnection.Find(new RiakFindRequest { Bucket = repoBucket, Keys = new[] { repoUrl }, ReadValue = 1 });
                if (riakResponse.Result.Count() == 1) repoInfo = riakResponse.Result.Select(document => jsonBridge.Deserialize<RepoInfo>(document.Value)).Single();
            }
            catch (InvalidOperationException e)
            {
                return new List<BuildHistoryItem>();
            }
            return repoInfo.BuildHistory.Select(guid => new BuildHistoryItem() {BuildId = guid}).ToList();
        }

        public void SetBuildStarted(Guid id, IEnumerable<TaskInfo> taskInfos)
        {
            var a = GetBuildStatus(id);
            a.BuildStarted(taskInfos);
            var connectionManager = GetConnectionManager();
            var riakConnection = new RiakContentRepository(connectionManager);
            riakConnection.Persist(new RiakPersistRequest { Bucket = idsBucket, Key = id.ToString(), Content = new RiakContent { Value = jsonBridge.Serialize(a).GetBytes() } });
        }

        public void WipeBucket()
        {
            RiakConnectionManager connectionManager = GetConnectionManager();
            WipeBuckett(connectionManager, idsBucket);
            WipeBuckett(connectionManager, repoBucket);
        }

        public void BuildUpdated(Guid id, int taskIndex, TaskInfo.TaskStatus taskStatus)
        {
            var a = GetBuildStatus(id);
            a.BuildUpdated(taskIndex, taskStatus);
            var connectionManager = GetConnectionManager();
            var riakConnection = new RiakContentRepository(connectionManager);
            riakConnection.Persist(new RiakPersistRequest { Bucket = idsBucket, Key = id.ToString(), Content = new RiakContent { Value = jsonBridge.Serialize(a).GetBytes() } });
        }

        public void BuildEnded(Guid id, BuildTotalEndStatus totalStatus)
        {
            var a = GetBuildStatus(id);
            a.BuildEnded(totalStatus);
            var connectionManager = GetConnectionManager();
            var riakConnection = new RiakContentRepository(connectionManager);
            riakConnection.Persist(new RiakPersistRequest { Bucket = idsBucket, Key = id.ToString(), Content = new RiakContent { Value = jsonBridge.Serialize(a).GetBytes() } });
        }

        public void AppendTerminalOutput(Guid buildId, int taskIndex, int contentSequenceIndex, string content)
        {
            var a = GetBuildStatus(buildId);
            a.Tasks[taskIndex].AddTerminalOutput(contentSequenceIndex, content);
            var connectionManager = GetConnectionManager();
            var riakConnection = new RiakContentRepository(connectionManager);
            riakConnection.Persist(new RiakPersistRequest { Bucket = idsBucket, Key = buildId.ToString(), Content = new RiakContent { Value = jsonBridge.Serialize(a).GetBytes() } });
        }

        private void WipeBuckett(RiakConnectionManager connectionManager, string bucket)
        {
            var riakConnection1 = new RiakBucketRepository(connectionManager);
            var keysFor = riakConnection1.ListKeysFor(new ListKeysRequest {Bucket = bucket.GetBytes()});
            var riakConnection = new RiakContentRepository(connectionManager);
            keysFor.Result.ToList().ForEach(
                key => riakConnection.Detach(new RiakDetachRequest() {Bucket = bucket, Key = key}));
        }

        private RiakConnectionManager GetConnectionManager()
        {
            var connectionManager = RiakConnectionManager.FromConfiguration;
            connectionManager.AddConnection(host, port);
            return connectionManager;
        }
    }

    public class RepoInfo
    {
        public Guid CurrentBuild;
        public List<Guid> BuildHistory = new List<Guid>();
    }
}