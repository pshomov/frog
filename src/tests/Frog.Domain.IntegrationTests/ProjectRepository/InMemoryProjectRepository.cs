using Frog.Domain.RepositoryTracker;

namespace Frog.Domain.IntegrationTests.ProjectRepository
{
    class InMemoryProjectRepository : ProjectRepository
    {
        protected override IProjectsRepository GetProjectRepository()
        {
            return new InMemoryProjectsRepository();
        }
    }
}