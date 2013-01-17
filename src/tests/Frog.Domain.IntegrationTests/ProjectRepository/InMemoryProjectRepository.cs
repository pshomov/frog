using Frog.Domain.RepositoryTracker;
using NUnit.Framework;

namespace Frog.Domain.IntegrationTests.ProjectRepository
{
	[TestFixture]
    public class InMemoryProjectRepository : ProjectRepository
    {
        protected override IProjectsRepository GetProjectRepository()
        {
            return new InMemoryProjectsRepository();
        }
    }
}