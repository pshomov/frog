using System;
using Frog.Domain.RepositoryTracker;
using Frog.Support;
using NUnit.Framework;

namespace Frog.Domain.IntegrationTests.ProjectRepository
{
	[TestFixture]
    public class RiakProjectRepositorySpec : ProjectRepositorySpec
    {
	    private const string Bucket = "projects_test1";

        protected override ProjectsRepository GetProjectsRepository()
        {
            return new Integration.RiakProjectRepository(OSHelpers.RiakHost(), OSHelpers.RiakPort(), Bucket);
        }
    }
}