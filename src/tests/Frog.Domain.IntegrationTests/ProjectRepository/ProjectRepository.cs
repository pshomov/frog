using System;
using System.Linq;
using Frog.Domain.RepositoryTracker;
using NUnit.Framework;

namespace Frog.Domain.IntegrationTests.ProjectRepository
{
    [TestFixture]
    public abstract class ProjectRepository
    {
        private IProjectsRepository riakProject;
        private string projectId;

        [SetUp]
        public void Setup()
        {
            riakProject = GetProjectRepository();
            projectId = Guid.NewGuid().ToString();
            riakProject.WipeBucket();
        }

        protected abstract IProjectsRepository GetProjectRepository();

        [Test]
        public void should_find_document_by_id()
        {
            riakProject.TrackRepository(projectId);
            Assert.That(riakProject.AllProjects.Where(document => document.projecturl == projectId).Count(), Is.EqualTo(1));
            Assert.That(riakProject.AllProjects.Single(document => document.projecturl == projectId).revision, Is.Empty);
        }

        [Test]
        public void should_update_repo_revision_when_new_one_specified()
        {
            riakProject.TrackRepository(projectId);
            riakProject.UpdateLastKnownRevision(projectId, "123");
            Assert.That(riakProject.AllProjects.Single(document => document.projecturl == projectId).revision, Is.EqualTo("123"));
        }

        [Test]
        public void should_not_have_project_listed_after_its_deleted()
        {
            riakProject.TrackRepository(projectId);
            riakProject.RemoveProject(projectId);
            Assert.That(riakProject.AllProjects.Where(document => document.projecturl == projectId).Count(), Is.EqualTo(0));
        }

        [Test]
        public void should_mark_project_as_CHECK_COMPLETE_when_updateing_revision_number()
        {
            riakProject.TrackRepository(projectId);
            riakProject.ProjectCheckInProgress(projectId);
            riakProject.UpdateLastKnownRevision(projectId, "as");
            Assert.That(riakProject.AllProjects.Single(document => document.projecturl == projectId).CheckForUpdateRequested, Is.False);
        }

        [Test]
        public void should_mark_project_as_CHECK_COMPLETE_when_check_is_being_marked_as_complete()
        {
            riakProject.TrackRepository(projectId);
            riakProject.ProjectCheckInProgress(projectId);
            riakProject.ProjectCheckComplete(projectId);
            Assert.That(riakProject.AllProjects.Single(document => document.projecturl == projectId).CheckForUpdateRequested, Is.False);
        }

        [Test]
        public void should_get_empty_list_of_projects_when_none_is_tracked()
        {
            Assert.That(riakProject.AllProjects.Count(), Is.EqualTo(0));            
        }

    }
}