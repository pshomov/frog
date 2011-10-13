using Frog.Domain.RepositoryTracker;
using NUnit.Framework;

namespace Frog.Domain.IntegrationTests.ProjectRepository
{
	[TestFixture]
    public class RiakProjectRepository : ProjectRepository
    {
        protected const string Bucket = "projects_test1";
        protected const string RiakServer = "127.0.0.1";
        protected const int RiakPort = 8087;

        protected override IProjectsRepository GetProjectRepository()
        {
            return new Integration.RiakProjectRepository(RiakServer, RiakPort, Bucket);
        }
    }
}