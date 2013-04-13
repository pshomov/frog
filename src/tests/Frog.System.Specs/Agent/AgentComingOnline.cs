using System;
using Frog.Domain;
using Frog.Domain.Underware;
using Frog.Specs.Support;
using Frog.Support;
using Frog.System.Specs.Underware;
using NSubstitute;
using NUnit.Framework;
using SaaS.Client.Projections.Frog.Projects;
using SaaS.Engine;
using xray;
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
    public class AgentComingOnline : BDD
    {
        private const string RepoUrl = "123";
        private SystemDriver system;
        private Guid newGuid;
        private Guid taskGuid;
        private TestSystem testSystem;

        protected override void Given()
        {
            taskGuid = Guid.NewGuid();
            testSystem = new TestSystem();
            system = new SystemDriver(testSystem);
        }

        protected override void When()
        {
            var sourceRepoDriver = Substitute.For<SourceRepoDriver>();
            var workingAreaGoverner = Substitute.For<WorkingAreaGoverner>();
            testSystem.SetupAgent(url => sourceRepoDriver, workingAreaGoverner);
        }

        [Test]
        public void should_announce_the_project_has_been_checked_out()
        {
            var prober = new PollingProber(5000, 100);
            Assert.True(prober.check(Take.Snapshot(() => system.GetEventsSnapshot())
                                         .Has(x => x,
                                              An.Event<AgentJoined>(
                                                  ))));
        }

        protected override void GivenCleanup()
        {
            system.Stop();
        }

    }
}