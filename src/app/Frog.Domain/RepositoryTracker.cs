using System;
using System.Linq;
using SimpleCQRS;

namespace Frog.Domain
{
    public class RepositoryTracker : Handles<UpdateFound>, Handles<RegisterRepository>, Handles<CheckForUpdateFailed>
    {
        public RepositoryTracker(IBus bus, ProjectsRepository projectsRepository)
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
            bus.Publish(new RepositoryRegistered(){RepoUrl = message.Repo});
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
                bus.Send(new BuildRequest {RepoUrl = message.RepoUrl, Revision = message.Revision, Id = Guid.NewGuid(), CapabilitiesNeeded = new string[]{}});
        }

        public void Handle(CheckForUpdateFailed message)
        {
            projectsRepository.ProjectCheckComplete(message.RepoUrl);
        }

        public void JoinTheMessageParty()
        {
            bus.RegisterHandler<UpdateFound>(Handle, RepositoryTrackerQueueId);
            bus.RegisterHandler<RegisterRepository>(Handle, RepositoryTrackerQueueId);
            bus.RegisterHandler<CheckForUpdateFailed>(Handle, RepositoryTrackerQueueId);
        }

        const string RepositoryTrackerQueueId = "Repository_tracker";
        readonly IBus bus;
        readonly ProjectsRepository projectsRepository;

        void Track(string repoUrl)
        {
            projectsRepository.TrackRepository(repoUrl);
        }
    }
}