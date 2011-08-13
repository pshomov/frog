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

    public class RepositoryTracker : Handles<UpdateFound>, Handles<RegisterRepository>, Handles<CheckForUpdateFailed>
    {
        private const string RepositoryTrackerQueueId = "Repository_tracker";
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
            projectsRepository.AllProjects.Where(document => !document.CheckForUpdateRequested). ToList().ForEach(
                 document =>
                     {
                         projectsRepository.ProjectCheckInProgress(document.projecturl);
                         bus.Send(new CheckForUpdates(repoUrl: document.projecturl, revision: document.revision));
                     });
        }

        public void JoinTheMessageParty()
        {
            bus.RegisterHandler<UpdateFound>(Handle, RepositoryTrackerQueueId);
            bus.RegisterHandler<RegisterRepository>(Handle, RepositoryTrackerQueueId);
            bus.RegisterHandler<CheckForUpdateFailed>(Handle, RepositoryTrackerQueueId);
        }

        public void Handle(RegisterRepository message)
        {
            Track(message.Repo);
        }

        public void Handle(UpdateFound message)
        {
            projectsRepository.UpdateLastKnownRevision(message.RepoUrl, message.Revision);
        }

        public void Handle(CheckForUpdateFailed message)
        {
            projectsRepository.ProjectCheckComplete(message.repoUrl);
        }
    }
}