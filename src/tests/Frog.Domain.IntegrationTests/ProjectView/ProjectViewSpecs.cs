using System;
using System.Collections.Generic;
using Frog.Domain.UI;
using Frog.Support;
using NUnit.Framework;

namespace Frog.Domain.IntegrationTests.ProjectView
{
    [TestFixture]
    public abstract class ProjectViewSpecs
    {
        private UI.ProjectView view;
        BuildView build_view;

        [SetUp]
        public void Setup()
        {
            view = GetProjectView();
            build_view = view as BuildView;
            ((ProjectTestSupport) view).WipeBucket();
        }


        protected abstract UI.ProjectView GetProjectView();

        [Test]
        public void should_have_even_for_nonexisting_builds()
        {
            try
            {
                var buildStatus = build_view.GetBuildStatus(Guid.NewGuid());
                Assert.Fail("should come here for builds that do not exist");
            }
            catch (BuildNotFoundException)
            {
            }
        }

        [Test]
        public void should_persist_build_status_when_build_started()
        {
            var id = Guid.NewGuid();
            view.SetCurrentBuild("ldsfslkdf", id, "", "");
            build_view.SetBuildStarted(id, new List<TaskInfo>());

            Assert.That(build_view.GetBuildStatus(id).Overall, Is.EqualTo(BuildTotalStatus.BuildStarted));
        }

        [Test]
        public void should_persist_build_status_when_build_updated()
        {
            var id = Guid.NewGuid();
            view.SetCurrentBuild("ldsfslkdf", id, "", "");
            build_view.SetBuildStarted(id, As.List(new TaskInfo(){Name = "t1", Status = TaskInfo.TaskStatus.NotStarted}));
            build_view.BuildUpdated(id, 0, TaskInfo.TaskStatus.FinishedSuccess);

            Assert.That(build_view.GetBuildStatus(id).Tasks[0].Status, Is.EqualTo(TaskInfo.TaskStatus.FinishedSuccess));
        }

        [Test]
        public void should_persist_build_status_when_build_ends()
        {
            var id = Guid.NewGuid();
            view.SetCurrentBuild("ldsfslkdf", id, "", "");
            build_view.SetBuildStarted(id, As.List(new TaskInfo {Name = "t1", Status = TaskInfo.TaskStatus.NotStarted}));
            build_view.BuildUpdated(id, 0, TaskInfo.TaskStatus.FinishedSuccess);
            build_view.BuildEnded(id,BuildTotalEndStatus.Error);

            Assert.That(build_view.GetBuildStatus(id).Overall, Is.EqualTo(BuildTotalStatus.BuildEndedError));
        }

        [Test]
        public void should_throw_exception_when_trying_to_get_a_build_for_repo_that_has_not_been_registered()
        {
            try
            {
                view.GetCurrentBuild("http:/asdasda");
                Assert.Fail("repo is not registered");
            }
            catch (RepositoryNotRegisteredException)
            {
            }
        }

        [Test]
        public void should_set_current_build_for_repos_that_dont_exist()
        {
            const string repoUrl = "http:///";
            var newGuid = Guid.NewGuid();
            view.SetCurrentBuild(repoUrl, newGuid, "c", "12");            
            Assert.That(view.GetCurrentBuild(repoUrl), Is.EqualTo(newGuid));
        }

        [Test]
        public void should_tell_when_project_is_not_registered()
        {
            Assert.That(view.IsProjectRegistered("htpp:///"), Is.False);
        }

        [Test]
        public void should_tell_when_project_is_registered()
        {
            view.SetCurrentBuild("http:///", Guid.NewGuid(), "", "");
            Assert.That(view.IsProjectRegistered("http:///"), Is.True);
        }

        [Test]
        public void should_list_of_builds_comes_back_empty_if_no_builds_yet()
        {
            Assert.That(view.GetListOfBuilds("http://").Count, Is.EqualTo(0));
        }

        [Test]
        public void should_have_builds_in_the_list_of_builds()
        {
            const string repoUrl = "http:///";
            var id1 = Guid.NewGuid();
            view.SetCurrentBuild(repoUrl, id1, "", "");
            var id2 = Guid.NewGuid();
            view.SetCurrentBuild(repoUrl, id2, "", "");
            var id3 = Guid.NewGuid();
            view.SetCurrentBuild(repoUrl, id3, "", "");

            var buildHistoryItems = view.GetListOfBuilds(repoUrl);

            Assert.That(buildHistoryItems.Count, Is.EqualTo(3));
            Assert.That(buildHistoryItems[0].BuildId, Is.EqualTo(id1));
            Assert.That(buildHistoryItems[1].BuildId, Is.EqualTo(id2));
            Assert.That(buildHistoryItems[2].BuildId, Is.EqualTo(id3));
        }

    }
}