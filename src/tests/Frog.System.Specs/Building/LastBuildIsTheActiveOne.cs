﻿using System;
using Frog.Domain;
using Frog.Domain.BuildSystems.FrogSystemTest;
using Frog.Domain.CustomTasks;
using Frog.Domain.UI;
using Frog.Specs.Support;
using Frog.Support;
using Frog.System.Specs.Underware;
using NSubstitute;
using NUnit.Framework;
using xray;

namespace Frog.System.Specs.Building
{
    [TestFixture]
    public class LastBuildIsTheActiveOne : BDD
    {
        private const string RepoUrl = "http://123";
        private SystemDriver system;
        private Guid newGuid;

        protected override void Given()
        {
            var sourceRepoDriver = Substitute.For<SourceRepoDriver>();
            var workingAreaGoverner = Substitute.For<WorkingAreaGoverner>();
            workingAreaGoverner.AllocateWorkingArea().Returns("fake location");
            var testSystem = new TestSystem(workingAreaGoverner, url => sourceRepoDriver);
            testSystem.TasksSource.Detect(Arg.Any<string>()).Returns(
                As.List(
                    (ITask)
                    new FakeTaskDescription("fle")));
            system = new SystemDriver(testSystem);
            system.RegisterNewProject(RepoUrl);
        }

        protected override void When()
        {
            newGuid = Guid.NewGuid();
            system.Build(RepoUrl, "123", newGuid);
            newGuid = Guid.NewGuid();
            system.Build(RepoUrl, "123", newGuid);
        }

        [Test]
        public void should_make_the_last_build_the_current_one()
        {
            var prober = new PollingProber(5000, 100);
            Assert.True(prober.check(Take.Snapshot(() => system.GetView())
                                         .Has(x => x,
                                              A.Check<ProjectView>(view => view.GetCurrentBuild(RepoUrl) == newGuid))));
        }

    }
}