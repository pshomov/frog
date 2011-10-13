using System;
using Frog.Domain.RepositoryTracker;
using NUnit.Framework;

namespace Frog.Domain.IntegrationTests.ProjectRepository
{
	[TestFixture]
    public class RiakProjectRepository : ProjectRepository
    {
	    private const string Bucket = "projects_test1";
	    private const int RiakPort = 8087;

        protected override IProjectsRepository GetProjectRepository()
        {
            return new Integration.RiakProjectRepository(Environment.GetEnvironmentVariable("RUNZ_RIAK_HOST") ?? "localhost", RiakPort, Bucket);
        }
    }
}