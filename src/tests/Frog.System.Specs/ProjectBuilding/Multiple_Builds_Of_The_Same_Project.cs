using System;
using Frog.Domain;
using Frog.Support;
using Frog.System.Specs.Underware;
using NSubstitute;
using NUnit.Framework;
using SaaS.Client.Projections.Frog.Projects;
using SaaS.Engine;
using xray;
using CheckoutInfo = Frog.Domain.CheckoutInfo;

namespace Frog.System.Specs.ProjectBuilding
{
    [TestFixture]
    public class Multiple_Builds_Of_The_Same_Project : SystemBDD
    {
        protected override void Given()
        {
            base.Given();

            var sourceRepoDriver = Substitute.For<SourceRepoDriver>();
            sourceRepoDriver.GetSourceRevision(Arg.Any<string>(), Arg.Any<string>()).Returns(new CheckoutInfo{Comment = "comment 1"}, new CheckoutInfo {Comment = "comment 2"});
            var workingAreaGoverner = Substitute.For<WorkingAreaGoverner>();
            workingAreaGoverner.AllocateWorkingArea().Returns("fake location");

            testSystem
                .WithProjections()
                .WithRepositoryTracker()
                .WithRevisionChecker(url => sourceRepoDriver)
                .AddAgent(url => sourceRepoDriver, workingAreaGoverner, Guid.NewGuid(), new string[] {});

            bool shouldStop;
            testSystem.TasksSource.Detect(Arg.Any<string>(), out shouldStop).Returns(
                As.List(
                    (TaskDescription)
                    new FakeTaskDescription("fle")));

            system.RegisterNewProject(RepoUrl);
        }

        protected override void When()
        {
            buildId1 = Guid.NewGuid();
            system.Build(RepoUrl, new RevisionInfo { Revision = "123" }, buildId1);
            buildId2 = Guid.NewGuid();
            system.Build(RepoUrl, new RevisionInfo { Revision = "123" }, buildId2);
        }


        [Test]
        public void should_make_the_last_build_the_current_one()
        {
            var prober = new PollingProber(5000, 100);
            Assert.True(prober.check(Take.Snapshot(() => system.GetView<ProjectId, ProjectHistory>(new ProjectId(RepoUrl)))
                                         .Has(A.Check<ProjectHistory>(view => view.Current.buildId == new BuildId(buildId2)))));
        }

        [Test]
        public void should_have_the_second_last_in_the_list_of_builds()
        {
            var prober = new PollingProber(5000, 100);
            Assert.True(prober.check(Take.Snapshot(() => system.GetView<ProjectId, ProjectHistory>(new ProjectId(RepoUrl)))
                                         .Has(A.Check<ProjectHistory>(view => view.Current.buildId == new BuildId(buildId2)))
                                         .Has(A.Check<ProjectHistory>(view => view.Items.Count == 1 && view.Items[0].BuildId == new BuildId(buildId1)))));
        }

        [Test]
        public void should_have_the_commit_messages_associated_with_the_build_history_items()
        {
            var prober = new PollingProber(5000, 100);
            Assert.True(prober.check(Take.Snapshot(() => system.GetView<ProjectId, ProjectHistory>(new ProjectId(RepoUrl)))
                                         .Has(A.Check<ProjectHistory>(view => view.CurrentHistory.RevisionComment == "comment 2"))
                                         .Has(A.Check<ProjectHistory>(view => view.Items.Count == 1 && view.Items[0].RevisionComment == "comment 1"))));
        }

        private const string RepoUrl = "http://123";
        private Guid buildId1;
        private Guid buildId2;
    }

}