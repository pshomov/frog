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
        [Test]
        public void should_find_document_by_id()
        {
            var riakProject = new RiakProjectRepository();
            riakProject.TrackRepository("http://fle");
            Assert.That(riakProject.AllProjects.Count(), Is.EqualTo(1));
            Assert.That(riakProject.AllProjects.ToArray()[0].Url, Is.EqualTo("http://fle"));
            Assert.That(riakProject.AllProjects.ToArray()[0].LastBuiltRevision, Is.Empty);
        }

    }
}