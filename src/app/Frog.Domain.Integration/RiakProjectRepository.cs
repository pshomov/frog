using System.Collections.Generic;
using System.Linq;
using Frog.Domain.RepositoryTracker;

namespace Frog.Domain.Integration
{
    public class RiakProjectRepository : IProjectsRepository
    {
        private readonly string host;
        private readonly int port;
        private readonly string bucket;

        public RiakProjectRepository(string host, int port, string bucket)
        {
            this.host = host;
            this.port = port;
            this.bucket = bucket;
        }

        public void TrackRepository(string repoUrl)
        {
            var client = Riak.GetConnectionManager(host, port).CreateClient();
            client.Put(bucket, Riak.KeyGenerator(repoUrl),
                       new RepositoryDocument {revision = "", projecturl = repoUrl});
        }

        public IEnumerable<RepositoryDocument> AllProjects
        {
            get
            {
                var client = Riak.GetConnectionManager(host, port).CreateClient();
                var keys = client.ListKeyz(bucket);
                return keys.Select(s => client.Get<RepositoryDocument>(bucket, s));
            }
        }

        public void UpdateLastKnownRevision(string repoUrl, string revision)
        {
            var connectionManager = Riak.GetConnectionManager(host, port).CreateClient();
            var doc = connectionManager.Get<RepositoryDocument>(bucket, Riak.KeyGenerator(repoUrl));
            doc.revision = revision;
            doc.CheckForUpdateRequested = false;
            connectionManager.Put(bucket, Riak.KeyGenerator(repoUrl), doc);
        }

        public void RemoveProject(string repoUrl)
        {
            var connectionManager = Riak.GetConnectionManager(host, port).CreateClient();
            connectionManager.Delete(bucket, Riak.KeyGenerator(repoUrl));
        }

        public void ProjectCheckInProgress(string repoUrl)
        {
            var client = Riak.GetConnectionManager(host, port).CreateClient();
            var doc = client.Get<RepositoryDocument>(bucket, Riak.KeyGenerator(repoUrl));
            doc.CheckForUpdateRequested = true;
            client.Put(bucket, Riak.KeyGenerator(repoUrl), doc);
        }

        public void ProjectCheckComplete(string repoUrl)
        {
            var client = Riak.GetConnectionManager(host, port).CreateClient();
            var doc = client.Get<RepositoryDocument>(bucket, Riak.KeyGenerator(repoUrl));
            doc.CheckForUpdateRequested = false;
            client.Put(bucket, Riak.KeyGenerator(repoUrl), doc);
        }
    }
}