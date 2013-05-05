using System;
using Frog.Domain;
using Frog.System.Specs.Underware;
using NSubstitute;
using NUnit.Framework;
using xray;

namespace Frog.System.Specs.ProjectBuilding
{
    [TestFixture]
    public class Dispatcher_Refuses_Agent_Specialized_Build_Work : SystemBDD
    {
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
                .AddAgent(url => sourceRepoDriver, workingAreaGoverner, Guid.NewGuid());
        }

        protected override void When()
        {
            buildId = Guid.NewGuid();
            system.BuildRequest(RepoUrl, new RevisionInfo {Revision = "123"}, buildId, "special");
        }


        [Test]
        public void should_not_send_build_command_to_agent_when_special_capabilities_required()
        {
            Assert.False(EventStoreCheck(ES => ES.Has(A.Command<Build>(
                                                           ev =>
                                                           ev.Id == buildId && ev.RepoUrl == RepoUrl &&
                                                           ev.Revision.Revision == "123"))));
        }

        private const string RepoUrl = "123";
        private Guid buildId;
    }
}