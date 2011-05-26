using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Frog.Domain.Integration;
using NUnit.Framework;

namespace Frog.Domain.IntegrationTests.Riak
{
    [TestFixture]
    public class RiakStoringAndRetrievingById
    {
        private const string BUCKET = "projects_test1";
        private const string RIAK_SERVER = "10.0.2.2";
        private const int RIAK_PORT = 8087;

        private RiakProjectRepository riakProject;

        [SetUp]
        public void Setup()
        {
            riakProject = new RiakProjectRepository(RIAK_SERVER, RIAK_PORT, BUCKET);
        }

        [Test]
        public void should_find_document_by_id()
        {
            riakProject.TrackRepository("http://fle");
            Assert.That(riakProject.AllProjects.Count(), Is.EqualTo(1));
            Assert.That(riakProject.AllProjects.ToArray()[0].Url, Is.EqualTo("http://fle"));
            Assert.That(riakProject.AllProjects.ToArray()[0].LastBuiltRevision, Is.Empty);
        }

        [Test]
        public void should_update_repo_revision_when_new_one_specified()
        {
            riakProject.TrackRepository("http://fle");
            riakProject.UpdateLastKnownRevision("http://fle", "123");
            Assert.That(riakProject.AllProjects.ToArray()[0].LastBuiltRevision, Is.EqualTo("123"));
        }

    }
}