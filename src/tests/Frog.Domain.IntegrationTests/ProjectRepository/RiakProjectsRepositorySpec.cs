using System;
using Frog.Domain.RepositoryTracker;
using Frog.Support;
using NUnit.Framework;

namespace Frog.Domain.IntegrationTests.ProjectRepository
{
	[TestFixture]
    public class RiakProjectsRepositorySpec : ProjectsRepositorySpec
    {
	    private const string Bucket = "projects_test1";

        protected override Tuple<ProjectsRepository, ProjectsRepositoryTestSupport> GetProjectsRepository()
        {
            var riakProjectRepository = new Integration.RiakProjectRepository(OSHelpers.RiakHost(), OSHelpers.RiakPort(), Bucket);
            return new Tuple<ProjectsRepository, ProjectsRepositoryTestSupport>(riakProjectRepository, riakProjectRepository);
        }
    }
}