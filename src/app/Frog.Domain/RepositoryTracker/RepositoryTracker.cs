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
        public string RepoUrl { get; set; }
        public string Revision { get; set; }

        public Build()
        {
        }

        public Build(string repoUrl, string revision)
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
                         bus.Send(new CheckRevision{RepoUrl = document.projecturl});
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
            var currentRev =
                projectsRepository.AllProjects.First(document => document.projecturl == message.RepoUrl).revision;
            projectsRepository.UpdateLastKnownRevision(message.RepoUrl, message.Revision);
            if (currentRev != message.Revision) bus.Send(new Build {RepoUrl = message.RepoUrl, Revision = message.Revision});
        }

        public void Handle(CheckForUpdateFailed message)
        {
            projectsRepository.ProjectCheckComplete(message.repoUrl);
        }
    }
}