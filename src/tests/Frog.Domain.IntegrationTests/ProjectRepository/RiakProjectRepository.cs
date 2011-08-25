using Frog.Domain.RepositoryTracker;

namespace Frog.Domain.IntegrationTests.ProjectRepository
{
    class RiakProjectRepository : ProjectRepository
    {
        protected const string Bucket = "projects_test1";
        protected const string RiakServer = "10.0.2.2";
        protected const int RiakPort = 8087;

        protected override IProjectsRepository GetProjectRepository()
        {
            return new Integration.RiakProjectRepository(RiakServer, RiakPort, Bucket);
        }
    }
}