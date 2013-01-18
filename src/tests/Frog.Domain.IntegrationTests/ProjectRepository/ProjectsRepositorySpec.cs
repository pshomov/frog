using System;
using System.Linq;
using Frog.Domain.RepositoryTracker;
using NUnit.Framework;

namespace Frog.Domain.IntegrationTests.ProjectRepository
{
    [TestFixture]
    public abstract class ProjectsRepositorySpec
    {
        ProjectsRepository projectsRepository;
        string projectId;

        [SetUp]
        public void Setup()
        {
            projectsRepository = GetProjectsRepository();
            projectId = Guid.NewGuid().ToString();
            projectsRepository.WipeBucket();
        }

        protected abstract ProjectsRepository GetProjectsRepository();

        [Test]
        public void should_find_document_by_id()
        {
            projectsRepository.TrackRepository(projectId);
            Assert.That(projectsRepository.AllProjects.Count(document => document.projecturl == projectId), Is.EqualTo(1));
            Assert.That(projectsRepository.AllProjects.Single(document => document.projecturl == projectId).revision, Is.Empty);
        }

        [Test]
        public void should_update_repo_revision_when_new_one_specified()
        {
            projectsRepository.TrackRepository(projectId);
            projectsRepository.UpdateLastKnownRevision(projectId, "123");
            Assert.That(projectsRepository.AllProjects.Single(document => document.projecturl == projectId).revision,
                        Is.EqualTo("123"));
        }

        [Test]
        public void should_not_have_project_listed_after_its_deleted()
        {
            projectsRepository.TrackRepository(projectId);
            projectsRepository.RemoveProject(projectId);
            Assert.That(projectsRepository.AllProjects.Count(document => document.projecturl == projectId), Is.EqualTo(0));
        }

        [Test]
        public void should_mark_project_as_CHECK_COMPLETE_when_updateing_revision_number()
        {
            projectsRepository.TrackRepository(projectId);
            projectsRepository.ProjectCheckInProgress(projectId);
            projectsRepository.UpdateLastKnownRevision(projectId, "as");
            Assert.That(
                projectsRepository.AllProjects.Single(document => document.projecturl == projectId).CheckForUpdateRequested,
                Is.False);
        }

        [Test]
        public void should_mark_project_as_CHECK_COMPLETE_when_check_is_being_marked_as_complete()
        {
            projectsRepository.TrackRepository(projectId);
            projectsRepository.ProjectCheckInProgress(projectId);
            projectsRepository.ProjectCheckComplete(projectId);
            Assert.That(
                projectsRepository.AllProjects.Single(document => document.projecturl == projectId).CheckForUpdateRequested,
                Is.False);
        }

        [Test]
        public void should_get_empty_list_of_projects_when_none_is_tracked()
        {
            Assert.That(projectsRepository.AllProjects.Count(), Is.EqualTo(0));
        }
    }
}