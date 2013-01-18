using Frog.Domain.RepositoryTracker;
using NUnit.Framework;

namespace Frog.Domain.IntegrationTests.ProjectRepository
{
	[TestFixture]
    public class InMemoryProjectRepositorySpec : ProjectRepositorySpec
    {
        protected override ProjectsRepository GetProjectsRepository()
        {
            return new InMemoryProjectsRepository();
        }
    }
}