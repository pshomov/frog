﻿using System;
using System.Threading;
using Frog.Domain;
using Frog.System.Specs.Underware;
using NSubstitute;
using NUnit.Framework;
using xray;

namespace Frog.System.Specs.ProjectBuilding
{
    [TestFixture]
    public class Dispatcher_Gives_Agent_Vanilla_Build_Work : SystemBDD
    {
        protected override void Given()
        {
            base.Given();
            var sourceRepoDriver = Substitute.For<SourceRepoDriver>();
            sourceRepoDriver.GetSourceRevision(Arg.Any<string>(), Arg.Any<string>())
                            .Returns(new CheckoutInfo() {Comment = "Fle", Revision = "123"});
            var workingAreaGoverner = Substitute.For<WorkingAreaGoverner>();
            workingAreaGoverner.AllocateWorkingArea().Returns("fake location");

            agentId = Guid.NewGuid();
            testSystem
                .WithProjections()
                .AddBuildDispatcher()
                .AddAgent(url => sourceRepoDriver, workingAreaGoverner, agentId);
            Thread.Sleep(7000);
        }

        protected override void When()
        {
            buildId = Guid.NewGuid();
            system.BuildRequest(RepoUrl, new RevisionInfo {Revision = "123"}, buildId, new string[] {});
        }

        [Test]
        public void should_send_build_command_to_agent_when_no_capabilities_required()
        {
            Assert.True(EventStoreCheck(agentId, ES => ES.Has(A.Command<Build>(
                                                           ev =>
                                                           ev.Id == buildId && ev.RepoUrl == RepoUrl &&
                                                           ev.Revision.Revision == "123"))));
        }

        private const string RepoUrl = "123";
        private Guid agentId;
        private Guid buildId;
    }
}