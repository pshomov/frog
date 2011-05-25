using System;
using System.Collections.Generic;
using System.Data.RiakClient;
using System.Data.RiakClient.Models;
using System.Linq;
using System.Security.Cryptography;
using System.Web.Script.Serialization;
using Frog.Domain.RepositoryTracker;
using riak.net.ProtoModels;

namespace Frog.Domain.Integration
{
    public class RiakProjectRepository : IProjectsRepository
    {
        private const string BUCKET = "projects_test1";
        readonly JavaScriptSerializer jsonBridge = new JavaScriptSerializer();

        public void TrackRepository(string repoUrl)
        {
            var connectionManager = GetConnectionManager();
            var riakConnection = new RiakContentRepository(connectionManager);
            var response = riakConnection.Persist(new RiakPersistRequest
                                                      {
                                                          Bucket = BUCKET,
                                                          Key = KeyGenerator(repoUrl),
                                                          Content = new RiakContent
                                                                        {
                                                                            Value = jsonBridge.Serialize(new RepositoryDocument{LastBuiltRevision = "" , Url = repoUrl}).GetBytes()
                                                                        }
                                                      }
                );
            if (response.ResponseCode != RiakResponseCode.Successful) throw new Exception("ouch, where is my data?");
        }

        public IEnumerable<RepositoryDocument> AllProjects
        {
            get
            {
                var connectionManager = GetConnectionManager();
                var riakConnection = new RiakBucketRepository(connectionManager);
                var keysFor = riakConnection.ListKeysFor(new ListKeysRequest {Bucket = BUCKET.GetBytes()});
                if (keysFor.ResponseCode != RiakResponseCode.Successful)
                    throw new Exception("ouch, where is my data?");
                var riakContentRepository = new RiakContentRepository(connectionManager);
                var riakResponse =
                    riakContentRepository.Find(new RiakFindRequest {Bucket = BUCKET, Keys = keysFor.Result, ReadValue = 1});
                return riakResponse.Result.Select(document => jsonBridge.Deserialize<RepositoryDocument>(document.Value));
            }
        }

        public void UpdateLastKnownRevision(string repoUrl, string revision)
        {
            RiakConnectionManager connectionManager = GetConnectionManager();
            var riakConnection = new RiakContentRepository(connectionManager);
            var riakResponse = riakConnection.Find(new RiakFindRequest { Bucket = BUCKET, Keys = new []{KeyGenerator(repoUrl)}, ReadValue = 1 });
            var doc = jsonBridge.Deserialize<RepositoryDocument>(riakResponse.Result[0].Value);
            doc.LastBuiltRevision = revision;
            riakConnection.Persist(new RiakPersistRequest {Bucket = BUCKET, Key = KeyGenerator(repoUrl), Content = new RiakContent {Value = jsonBridge.Serialize(doc).GetBytes()}});
        }

        private static RiakConnectionManager GetConnectionManager()
        {
            var connectionManager = RiakConnectionManager.FromConfiguration;
            connectionManager.AddConnection("10.0.2.2", 8087);
            return connectionManager;
        }

        private static string KeyGenerator(string repoUrl)
        {
            return string.Concat(
                new MD5CryptoServiceProvider().ComputeHash(
                    repoUrl.GetBytes()).Select(b => b.ToString("x2")));
        }
    }
}