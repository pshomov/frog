using System;
using System.Linq;
using SimpleCQRS;

namespace Frog.Domain.RepositoryTracker
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
        readonly IBus bus;
        readonly IProjectsRepository projectsRepository;

        public RepositoryTracker(IBus bus, IProjectsRepository projectsRepository)
        {
            this.bus = bus;
            this.projectsRepository = projectsRepository;
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
            projectsRepository.UpdateLastKnownRevision(message.RepoUrl, message.Revision);
        }
    }
}