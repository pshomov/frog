using System;
using System.Threading;
using Frog.Domain;
using Frog.System.Specs.Underware;
using NSubstitute;
using NUnit.Framework;
using xray;
using Build = Frog.Domain.Build;
using CheckoutInfo = Frog.Domain.CheckoutInfo;

namespace Frog.System.Specs.ProjectBuilding
{
    [TestFixture]
    public class Dispatcher_Gives_Agent_Vanilla_Build_Work : SystemBDD
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
                .WithProjections()
                .AddBuildDispatcher()
                .AddAgent(url => sourceRepoDriver, workingAreaGoverner);
            Thread.Sleep(700);
        }

        protected override void When()
        {
            buildId = Guid.NewGuid();
            system.BuildRequest(RepoUrl, new RevisionInfo {Revision = "123"}, buildId, new string[] {});
        }

        [Test]
        public void should_send_build_command_to_agent_when_no_capabilities_required()
        {
            Assert.True(EventStoreCheck(ES => ES.Has(A.Command<Build>(
                                                           ev =>
                                                           ev.Id == buildId && ev.RepoUrl == RepoUrl &&
                                                           ev.Revision.Revision == "123"))));
        }
    }
}