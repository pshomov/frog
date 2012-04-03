using System;
using System.Collections.Generic;
using Frog.Domain;
using Frog.Domain.BuildSystems.FrogSystemTest;
using Frog.Domain.UI;
using Frog.Specs.Support;
using Frog.Support;
using Frog.System.Specs.Underware;
using NSubstitute;
using NUnit.Framework;
using xray;

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
            var testSystem = new TestSystem(workingAreaGoverner, url => sourceRepoDriver);
            testSystem.TasksSource.Detect(Arg.Any<string>()).Returns(
                As.List(
                    (Task)
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

        [Test]
        public void should_make_the_last_build_the_current_one()
        {
            var prober = new PollingProber(5000, 100);
            Assert.True(prober.check(Take.Snapshot(() => system.GetProjectStatusView())
                                         .Has(x => x,
                                              A.Check<ProjectView>(view => view.GetCurrentBuild(RepoUrl) == newGuid))));
        }

        [Test]
        public void should_have_the_list_of_builds()
        {
            var prober = new PollingProber(5000, 100);
            Assert.True(prober.check(Take.Snapshot(() => system.GetProjectStatusView().GetListOfBuilds(RepoUrl))
                                         .Has(x => x,
                                              A.Check<List<BuildHistoryItem>>(
                                                  listOfBuilds =>
                                                  listOfBuilds.Count == 2 && listOfBuilds[0].BuildId == oldGuid &&
                                                  listOfBuilds[1].BuildId == newGuid))));
        }

        [Test]
        public void should_have_the_commit_messages_associated_with_the_build_history_items()
        {
            var prober = new PollingProber(5000, 100);
            Assert.True(prober.check(Take.Snapshot(() => system.GetProjectStatusView().GetListOfBuilds(RepoUrl))
                                         .Has(x => x,
                                              A.Check<List<BuildHistoryItem>>(
                                                  listOfBuilds =>
                                                  listOfBuilds.Count == 2 && listOfBuilds[0].Comment == "comment 1" &&
                                                  listOfBuilds[1].Comment == "comment 2"))));
        }
    }

}