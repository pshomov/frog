using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Frog.Domain.RepositoryTracker;
using Frog.Specs.Support;
using NSubstitute;
using NUnit.Framework;

namespace Frog.Domain.Specs.Agent
{
    [TestFixture]
    class AgentFailsRevsionCheck : AgentSpecsBase{
        protected override void Given()
        {
            base.Given();
            Worker.When(
                worker => worker.CheckForUpdatesAndKickOffPipeline(Arg.Any<SourceRepoDriver>(), Arg.Any<string>())).Do(
                    info =>
                    Worker.OnCheckForUpdateFailed += Raise.Event<Action>());
            repo.GetLatestRevision().Returns(info => {throw new NullReferenceException();});
        }

        protected override void When()
        {
            Agent.Handle(new Build(repoUrl: "http://fle", revision: "2"));            
        }

        [Test]
        public void should_send_event_that_check_failed()
        {
            Bus.Received().Publish(
                Arg.Any<CheckForUpdateFailed>());
        }

    }
}
