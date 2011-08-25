using System.Linq;
using Frog.Domain.RepositoryTracker;
using NUnit.Framework;

namespace Frog.Domain.IntegrationTests.ProjectRepository
{
    [TestFixture]
    public abstract class ProjectRepository
    {
        private IProjectsRepository riakProject;

        [SetUp]
        public void Setup()
        {
            riakProject = GetProjectRepository();
        }

        protected abstract IProjectsRepository GetProjectRepository();

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

        [Test]
        public void should_not_have_project_listed_after_its_deleted()
        {
            riakProject.TrackRepository("http://12123123");
            riakProject.RemoveProject("http://12123123");
            Assert.That(riakProject.AllProjects.Where(document => document.projecturl == "http://12123123").Count(), Is.EqualTo(0));
        }

    }
}