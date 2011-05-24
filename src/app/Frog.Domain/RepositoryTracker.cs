using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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

    public class ProjectsRepository
    {
        private readonly ConcurrentDictionary<string, RepositoryTracker.RepositoryInfo> trackedRepos;

        public ProjectsRepository()
        {
            trackedRepos = new ConcurrentDictionary<string, RepositoryTracker.RepositoryInfo>();
        }

        public void TrackRepository(string repoUrl)
        {
            trackedRepos.TryAdd(repoUrl, new RepositoryTracker.RepositoryInfo { Url = repoUrl, LastBuiltRevision = "" });
        }

        public RepositoryTracker.RepositoryInfo this[string repoUrl]{
            get { return trackedRepos[repoUrl]; }
        }

        public IEnumerable<RepositoryTracker.RepositoryInfo> AllProjects { get { return trackedRepos.Values; } }
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
        private ProjectsRepository projectsRepository;

        public RepositoryTracker(IBus bus)
        {
            this.bus = bus;
            projectsRepository = new ProjectsRepository();
        }

        private void Track(string repoUrl)
        {
            projectsRepository.TrackRepository(repoUrl);
        }

        public void CheckForUpdates()
        {
            projectsRepository.AllProjects.ToList().ForEach(
                s => bus.Send(new CheckForUpdates(repoUrl : s.Url, revision : s.LastBuiltRevision)));
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
            projectsRepository[message.RepoUrl].LastBuiltRevision = message.Revision;
        }
    }
}