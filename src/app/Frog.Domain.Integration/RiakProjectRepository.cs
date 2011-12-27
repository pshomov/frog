using System;
using System.Collections.Generic;
using System.Linq;
using CorrugatedIron;
using Frog.Domain.RepositoryTracker;

namespace Frog.Domain.Integration
{
    public class RiakProjectRepository : IProjectsRepository
    {
        private const string ALL_PROJECTS = "all_projects";
        private readonly string host;
        private readonly int port;
        private readonly string bucket;
        private IRiakClient iRiakClient;

        public RiakProjectRepository(string host, int port, string bucket)
        {
            this.host = host;
            this.port = port;
            this.bucket = bucket;
        }

        public void TrackRepository(string repoUrl)
        {
            var client = CreateClient();
            var repoHash = Riak.KeyGenerator(repoUrl);
            client.Put(bucket, repoHash,
                       new RepositoryDocument {revision = "", projecturl = repoUrl});
            var allProjects = new List<string>();
            try
            {
                allProjects = client.Get<List<string>>(bucket, ALL_PROJECTS);
            }
            catch (KeyNotFoundException)
            {
            }
            allProjects.Add(repoHash);
            client.Put(bucket, ALL_PROJECTS, allProjects);
        }

        public IEnumerable<RepositoryDocument> AllProjects
        {
            get
            {
                var client = CreateClient();
                List<string> keys;
                try
                {
                    keys = client.Get<List<string>>(bucket, ALL_PROJECTS);
                }
                catch (KeyNotFoundException)
                {
                    return new List<RepositoryDocument>();
                }
                return keys.Select(s => client.Get<RepositoryDocument>(bucket, s));
            }
        }

        public void UpdateLastKnownRevision(string repoUrl, string revision)
        {
            var connectionManager = CreateClient();
            var doc = connectionManager.Get<RepositoryDocument>(bucket, Riak.KeyGenerator(repoUrl));
            doc.revision = revision;
            doc.CheckForUpdateRequested = false;
            connectionManager.Put(bucket, Riak.KeyGenerator(repoUrl), doc);
        }

        public void RemoveProject(string repoUrl)
        {
            var client = CreateClient();
            var repoHash = Riak.KeyGenerator(repoUrl);
            client.Delete(bucket, repoHash);
            var all_projects = client.Get<List<string>>(bucket, ALL_PROJECTS);
            all_projects.Remove(repoHash);
            client.Put(bucket, ALL_PROJECTS, all_projects);
        }

        public void ProjectCheckInProgress(string repoUrl)
        {
            var client = CreateClient();
            var doc = client.Get<RepositoryDocument>(bucket, Riak.KeyGenerator(repoUrl));
            doc.CheckForUpdateRequested = true;
            client.Put(bucket, Riak.KeyGenerator(repoUrl), doc);
        }

        public void ProjectCheckComplete(string repoUrl)
        {
            var client = CreateClient();
            var doc = client.Get<RepositoryDocument>(bucket, Riak.KeyGenerator(repoUrl));
            doc.CheckForUpdateRequested = false;
            client.Put(bucket, Riak.KeyGenerator(repoUrl), doc);
        }

        public void WipeBucket()
        {
            var client = CreateClient();
            client.DeleteBucket(bucket);
        }

        private IRiakClient CreateClient()
        {
            return Riak.GetConnectionManager(host, port).CreateClient();
            if (iRiakClient == null) iRiakClient = Riak.GetConnectionManager(host, port).CreateClient();
            return iRiakClient;
        }
    }
}