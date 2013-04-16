using System;
using System.Collections.Generic;
using Frog.Domain;
using Frog.Specs.Support;
using Frog.Support;
using Frog.System.Specs.Underware;
using NSubstitute;
using NUnit.Framework;
using SaaS.Client.Projections.Frog.Projects;
using SaaS.Engine;
using SimpleCQRS;
using xray;
using Build = Frog.Domain.Build;
using BuildEnded = Frog.Domain.BuildEnded;
using BuildStarted = Frog.Domain.BuildStarted;
using BuildTotalEndStatus = Frog.Domain.BuildTotalEndStatus;
using BuildUpdated = Frog.Domain.BuildUpdated;
using CheckoutInfo = Frog.Domain.CheckoutInfo;
using ProjectCheckedOut = Frog.Domain.ProjectCheckedOut;
using TaskInfo = Frog.Domain.TaskInfo;

namespace Frog.System.Specs.ProjectBuilding
{
    [TestFixture]
    public class Dispatcher_Gives_Agent_Work : SystemBDD
    {
        private const string RepoUrl = "123";
        private Guid buildId;

        protected override void Given()
        {
            base.Given();
            var sourceRepoDriver = Substitute.For<SourceRepoDriver>();
            sourceRepoDriver.GetSourceRevision(Arg.Any<string>(), Arg.Any<string>())
                            .Returns(new CheckoutInfo() {Comment = "Fle", Revision = "123"});
            var workingAreaGoverner = Substitute.For<WorkingAreaGoverner>();
            workingAreaGoverner.AllocateWorkingArea().Returns("fake location");

            testSystem
                .AddBuildDispatcher()
                .SetupAgent(url => sourceRepoDriver, workingAreaGoverner);
            system = new SystemDriver(testSystem);
        }

        protected override void When()
        {
            buildId = Guid.NewGuid();
            system.BuildRequest(RepoUrl, new RevisionInfo {Revision = "123"}, buildId, new string[] {});
        }

        [Test]
        public void should_send_build_command_to_agent()
        {
            Assert.True(EventStoreCheck(ES => ES.Has(A.Command<Build>(
                                                           ev =>
                                                           ev.Id == buildId && ev.RepoUrl == RepoUrl &&
                                                           ev.Revision.Revision == "123"))));
        }
    }
}