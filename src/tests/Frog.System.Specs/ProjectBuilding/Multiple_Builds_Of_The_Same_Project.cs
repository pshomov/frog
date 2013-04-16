using System;
using Frog.Domain;
using Frog.Specs.Support;
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
    public class Multiple_Builds_Of_The_Same_Project : BDD
    {
        private const string RepoUrl = "http://123";
        private SystemDriver system;
        private Guid newGuid;
        private Guid oldGuid;

        protected override void Given()
        {
            var sourceRepoDriver = Substitute.For<SourceRepoDriver>();
            sourceRepoDriver.GetSourceRevision(Arg.Any<string>(), Arg.Any<string>()).Returns(new CheckoutInfo{Comment = "comment 1"}, new CheckoutInfo {Comment = "comment 2"});
            var workingAreaGoverner = Substitute.For<WorkingAreaGoverner>();
            workingAreaGoverner.AllocateWorkingArea().Returns("fake location");
            var testSystem = new TestSystem()
                .WithRepositoryTracker()
                .WithRevisionChecker(url => sourceRepoDriver)
                .SetupAgent(url => sourceRepoDriver, workingAreaGoverner, new string[] {})
                .SetupProjections();
            bool shouldStop;
            testSystem.TasksSource.Detect(Arg.Any<string>(), out shouldStop).Returns(
                As.List(
                    (TaskDescription)
                    new FakeTaskDescription("fle")));
            system = new SystemDriver(testSystem);
            system.RegisterNewProject(RepoUrl);
        }

        protected override void When()
        {
            newGuid = Guid.NewGuid();
            system.Build(RepoUrl, new RevisionInfo { Revision = "123" }, newGuid);
            oldGuid = newGuid;
            newGuid = Guid.NewGuid();
            system.Build(RepoUrl, new RevisionInfo { Revision = "123" }, newGuid);
        }

        protected override void GivenCleanup()
        {
            system.Stop();
        }

        [Test]
        public void should_make_the_last_build_the_current_one()
        {
            var prober = new PollingProber(5000, 100);
            Assert.True(prober.check(Take.Snapshot(() => system.GetView<ProjectId, ProjectHistory>(new ProjectId(RepoUrl)))
                                         .Has(x => x,
                                              A.Check<ProjectHistory>(view => view.Current.buildId == new BuildId(newGuid)))));
        }

        [Test]
        public void should_have_the_second_last_in_the_list_of_builds()
        {
            var prober = new PollingProber(5000, 100);
            Assert.True(prober.check(Take.Snapshot(() => system.GetView<ProjectId, ProjectHistory>(new ProjectId(RepoUrl)))
                                         .Has(x => x,
                                              A.Check<ProjectHistory>(view => view.Current.buildId == new BuildId(newGuid)))
                                         .Has(x => x,
                                              A.Check<ProjectHistory>(view => view.Items.Count == 1 && view.Items[0].BuildId == new BuildId(oldGuid)))));
        }

        [Test]
        public void should_have_the_commit_messages_associated_with_the_build_history_items()
        {
            var prober = new PollingProber(5000, 100);
            Assert.True(prober.check(Take.Snapshot(() => system.GetView<ProjectId, ProjectHistory>(new ProjectId(RepoUrl)))
                                         .Has(x => x,
                                              A.Check<ProjectHistory>(view => view.CurrentHistory.RevisionComment == "comment 2"))
                                         .Has(x => x,
                                              A.Check<ProjectHistory>(view => view.Items.Count == 1 && view.Items[0].RevisionComment == "comment 1"))));
        }
    }

}