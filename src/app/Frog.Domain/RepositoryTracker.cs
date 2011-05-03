using System;
using System.Collections.Concurrent;
using System.Linq;
using SimpleCQRS;

namespace Frog.Domain
{
    public class RegisterRepository : Command
    {
        public string Repo;
    }

    public class CheckForUpdates : Command
    {
        public string RepoUrl { get; set; }
        public string Revision { get; set; }

        public CheckForUpdates()
        {
        }

        public CheckForUpdates(string repoUrl, string revision)
        {
            RepoUrl = repoUrl;
            Revision = revision;
        }
    }

    public class RepositoryTracker : Handles<UpdateFound>, Handles<RegisterRepository>
    {
        public class RepositoryInfo
        {
            public RepositoryInfo()
            {
                Id = Guid.NewGuid();
            }

            public Guid Id { get; private set; }
            public string Url;
            public string LastBuiltRevision;
        }

        readonly IBus bus;
        readonly ConcurrentDictionary<string, RepositoryInfo> trackedRepos;

        public RepositoryTracker(IBus bus)
        {
            this.bus = bus;
            trackedRepos = new ConcurrentDictionary<string, RepositoryInfo>();
        }

        private void Track(string repoUrl)
        {
            trackedRepos.TryAdd(repoUrl, new RepositoryInfo {Url = repoUrl, LastBuiltRevision = ""});
        }

        public void CheckForUpdates()
        {
            trackedRepos.ToList().ForEach(
                s => bus.Send(new CheckForUpdates(repoUrl : s.Value.Url, revision : s.Value.LastBuiltRevision)));
        }

        public void JoinTheMessageParty()
        {
            bus.RegisterHandler<UpdateFound>(Handle, "Repository_tracker");
            bus.RegisterHandler<RegisterRepository>(Handle, "Repository_tracker");
        }

        public void Handle(RegisterRepository message)
        {
            Track(message.Repo);
        }

        public void Handle(UpdateFound message)
        {
            trackedRepos[message.RepoUrl].LastBuiltRevision = message.Revision;
        }
    }
}