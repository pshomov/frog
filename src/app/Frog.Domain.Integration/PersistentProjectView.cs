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
            var connectionManager = GetConnectionManager().CreateClient();
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
            var connectionManager = GetConnectionManager().CreateClient();
            var riakResponse =
                connectionManager.Get(repoBucket, KeyGenerator(repoUrl));
            if (riakResponse.IsSuccess && riakResponse.ResultCode == ResultCode.Success)
                return riakResponse.Value.GetObject<RepoInfo>().CurrentBuild;
            throw new RepositoryNotRegisteredException();
        }

        public void SetCurrentBuild(string repoUrl, Guid buildId, string comment, string revision)
        {
            var connectionManager = GetConnectionManager().CreateClient();
            var repoInfo = new RepoInfo();
            var riakResult = connectionManager.Get(repoBucket, KeyGenerator(repoUrl));
            if (riakResult.IsSuccess && riakResult.ResultCode != ResultCode.NotFound) repoInfo = riakResult.Value.GetObject<RepoInfo>();
            repoInfo.BuildHistory.Add(new BuildHistoryItem(){BuildId = buildId, Comment = comment, Revision = revision});
            repoInfo.CurrentBuild = buildId;
            connectionManager.Put(new RiakObject(repoBucket, KeyGenerator(repoUrl), repoInfo));
            connectionManager.Put(new RiakObject(idsBucket, buildId.ToString(), new BuildStatus()));
        }

        public bool ProjectRegistered(string projectUrl)
        {
            var riakContentRepository = GetConnectionManager().CreateClient();
            var riakResult = riakContentRepository.Get(repoBucket, KeyGenerator(projectUrl));
            return riakResult.IsSuccess;
        }

        public List<BuildHistoryItem> GetListOfBuilds(string repoUrl)
        {
            var connectionManager = GetConnectionManager().CreateClient();
            var repoInfo = new RepoInfo();
            var riakResponse =
                connectionManager.Get(repoBucket, KeyGenerator(repoUrl));
            if (riakResponse.IsSuccess && riakResponse.ResultCode == ResultCode.NotFound)
                return new List<BuildHistoryItem>();

            if (riakResponse.IsSuccess && riakResponse.ResultCode == ResultCode.Success)
                repoInfo = riakResponse.Value.GetObject<RepoInfo>();
            return repoInfo.BuildHistory.ToList();
        }

        public void SetBuildStarted(Guid id, IEnumerable<TaskInfo> taskInfos)
        {
            var a = GetBuildStatus(id);
            a.BuildStarted(taskInfos);

            var client = GetConnectionManager().CreateClient();
            client.Put(new RiakObject(idsBucket, id.ToString(), a));
            var b = GetBuildStatus(id);
			return;

//            var connectionManager = GetConnectionManager();
//            var riakConnection = new RiakContentRepository(connectionManager);
//            riakConnection.Persist(new RiakPersistRequest { Bucket = idsBucket, Key = id.ToString(), Content = new RiakContent { Value = jsonBridge.Serialize(a).GetBytes() }, Write = 1 });
        }

        public void WipeBucket()
        {
            var connectionManager = GetConnectionManager();
            WipeBuckett(connectionManager, idsBucket);
            WipeBuckett(connectionManager, repoBucket);
        }

        public void BuildUpdated(Guid id, int taskIndex, TaskInfo.TaskStatus taskStatus)
        {
            var a = GetBuildStatus(id);
            a.BuildUpdated(taskIndex, taskStatus);
            var riak = GetConnectionManager();
            riak.CreateClient().Put(new RiakObject(idsBucket, id.ToString(), a));
        }

        public void BuildEnded(Guid id, BuildTotalEndStatus totalStatus)
        {
            var a = GetBuildStatus(id);
            a.BuildEnded(totalStatus);
            var riak = GetConnectionManager();
            riak.CreateClient().Put(new RiakObject(idsBucket, id.ToString(), a));
        }

        public void AppendTerminalOutput(Guid buildId, int taskIndex, int contentSequenceIndex, string content)
        {
            var a = GetBuildStatus(buildId);
            a.Tasks[taskIndex].AddTerminalOutput(contentSequenceIndex, content);
            var riak = GetConnectionManager();
            riak.CreateClient().Put(new RiakObject(idsBucket, buildId.ToString(), a));
        }

        private void WipeBuckett(IRiakCluster connectionManager, string bucket)
        {
            var client = connectionManager.CreateClient();
            client.DeleteBucket(bucket);
        }

        private IRiakCluster GetConnectionManager()
        {
			if (cluster == null)
			{
			    var tempFileName = Path.GetTempFileName();
                File.AppendAllText(tempFileName, string.Format(@"<?xml version=""1.0"" encoding=""utf-8"" ?>
<configuration>
  <configSections>
    <section name=""riakConfig"" type=""CorrugatedIron.Config.RiakClusterConfiguration, CorrugatedIron""/>
  </configSections>
  <riakConfig nodePollTime=""5000"" defaultRetryWaitTime=""200"" defaultRetryCount=""3"">
    <nodes>
      <node name=""dev1"" hostAddress=""{0}"" pbcPort=""{1}"" restScheme=""http"" restPort=""8098"" poolSize=""20"" />
    </nodes>
  </riakConfig>
</configuration>
", host, port));
			    cluster = RiakCluster.FromConfig("riakConfig", tempFileName);
			}
            return cluster;
		}
		
		IRiakCluster cluster;
		
        private static string KeyGenerator(string repoUrl)
        {
            return string.Concat(
                new MD5CryptoServiceProvider().ComputeHash(
                    Encoding.UTF8.GetBytes(repoUrl)).Select(b => b.ToString("x2")));
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