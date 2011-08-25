using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Frog.Domain.Integration;
using Frog.Domain.RepositoryTracker;
using NUnit.Framework;

namespace Frog.Domain.IntegrationTests.Riak
{
    [TestFixture]
    public abstract class ProjectRepository
    {
        protected const string BUCKET = "projects_test1";
        protected const string RIAK_SERVER = "10.0.2.2";
        protected const int RIAK_PORT = 8087;

        private IProjectsRepository riakProject;

        [SetUp]
        public void Setup()
        {
            riakProject = GetProjectRepository();
        }

        protected abstract Integration.RiakProjectRepository GetProjectRepository();

        [Test]
        public void should_find_document_by_id()
        {
            riakProject.TrackRepository("http://fle");
            Assert.That(riakProject.AllProjects.Count(), Is.EqualTo(1));
            Assert.That(riakProject.AllProjects.ToArray()[0].projecturl, Is.EqualTo("http://fle"));
            Assert.That(riakProject.AllProjects.ToArray()[0].revision, Is.Empty);
        }

        [Test]
        public void should_update_repo_revision_when_new_one_specified()
        {
            riakProject.TrackRepository("http://fle");
            riakProject.UpdateLastKnownRevision("http://fle", "123");
            Assert.That(riakProject.AllProjects.ToArray()[0].revision, Is.EqualTo("123"));
        }

    }

    class RiakProjectRepository : ProjectRepository
    {
        protected override Integration.RiakProjectRepository GetProjectRepository()
        {
            return new Integration.RiakProjectRepository(RIAK_SERVER, RIAK_PORT, BUCKET);
        }
    }
}