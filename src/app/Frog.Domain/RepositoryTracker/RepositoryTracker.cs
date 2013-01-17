using System;
using System.Linq;
using Frog.Domain.RevisionChecker;
using SimpleCQRS;

namespace Frog.Domain.RepositoryTracker
{
    public class RegisterRepository : Command
    {
        public string Repo;
    }

    public class Build : Command
    {
        public Guid Id { get; set; }
        public string RepoUrl { get; set; }
        public RevisionInfo Revision { get; set; }

        public Build()
        {
            Id = Guid.NewGuid();
        }
    }

    public class RepositoryTracker : Handles<UpdateFound>, Handles<RegisterRepository>, Handles<CheckForUpdateFailed>
    {
        public RepositoryTracker(IBus bus, IProjectsRepository projectsRepository)
        {
            this.bus = bus;
            this.projectsRepository = projectsRepository;
        }

        public void CheckForUpdates()
        {
            projectsRepository.AllProjects.Where(document => !document.CheckForUpdateRequested).ToList().ForEach(
                document =>
                    {
                        projectsRepository.ProjectCheckInProgress(document.projecturl);
                        bus.Send(new CheckRevision {RepoUrl = document.projecturl});
                    });
        }

        public void Handle(RegisterRepository message)
        {
            Track(message.Repo);
        }

        public void Handle(UpdateFound message)
        {
            string currentRev;
            try
            {
                currentRev =
                    projectsRepository.AllProjects.First(document => document.projecturl == message.RepoUrl).revision;
            }
            catch (InvalidOperationException)
            {
                return;
            }
            projectsRepository.UpdateLastKnownRevision(message.RepoUrl, message.Revision.Revision);
            if (currentRev != message.Revision.Revision)
                bus.Send(new Build {RepoUrl = message.RepoUrl, Revision = message.Revision});
        }

        public void Handle(CheckForUpdateFailed message)
        {
            projectsRepository.ProjectCheckComplete(message.repoUrl);
        }

        public void JoinTheMessageParty()
        {
            bus.RegisterHandler<UpdateFound>(Handle, RepositoryTrackerQueueId);
            bus.RegisterHandler<RegisterRepository>(Handle, RepositoryTrackerQueueId);
            bus.RegisterHandler<CheckForUpdateFailed>(Handle, RepositoryTrackerQueueId);
        }

        const string RepositoryTrackerQueueId = "Repository_tracker";
        readonly IBus bus;
        readonly IProjectsRepository projectsRepository;

        void Track(string repoUrl)
        {
            projectsRepository.TrackRepository(repoUrl);
        }
    }
}