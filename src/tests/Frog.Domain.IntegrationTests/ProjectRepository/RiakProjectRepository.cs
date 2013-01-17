using System;
using Frog.Domain.RepositoryTracker;
using Frog.Support;
using NUnit.Framework;

namespace Frog.Domain.IntegrationTests.ProjectRepository
{
	[TestFixture]
    public class RiakProjectRepository : ProjectRepository
    {
	    private const string Bucket = "projects_test1";

        protected override IProjectsRepository GetProjectRepository()
        {
            return new Integration.RiakProjectRepository(OSHelpers.RiakHost(), OSHelpers.RiakPort(), Bucket);
        }
    }
}