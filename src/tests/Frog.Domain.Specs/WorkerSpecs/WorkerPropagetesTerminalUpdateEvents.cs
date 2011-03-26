using System;
using NSubstitute;
using NUnit.Framework;

namespace Frog.Domain.Specs.WorkerSpecs
{
    [TestFixture]
    public class WorkerPropagetesTerminalUpdateEvents : WorkerSpecsBase
    {
        string content;
        int taskindex;
        int contentSequenceIndex;

        protected override void Given()
        {
            base.Given();
            SourceRepoDriver.GetLatestRevision().Returns("2344");
            WorkingAreaGovernor.AllocateWorkingArea().Returns("dugh");
            Worker = new Worker(Pipeline, WorkingAreaGovernor);
            Worker.OnTerminalUpdates += info =>
                                            {
                                                content = info.Content;
                                                taskindex = info.TaskIndex;
                                                contentSequenceIndex = info.ContentSequenceIndex;
                                            };
            Pipeline.When(pipeline => pipeline.Process(Arg.Any<SourceDrop>())).Do(
                info =>
                Pipeline.OnTerminalUpdate +=
                Raise.Event<Action<TerminalUpdateInfo>>(new TerminalUpdateInfo(content: "cont", taskIndex: 2,
                                                                               contentSequenceIndex: 3)));
        }

        protected override void When()
        {
            Worker.CheckForUpdatesAndKickOffPipeline(repositoryDriver: SourceRepoDriver, revision: "123");
        }

        [Test]
        public void should_propagate_message_from_pipleine_as_is()
        {
            Assert.That(content, Is.EqualTo("cont"));
            Assert.That(taskindex, Is.EqualTo(2));
            Assert.That(contentSequenceIndex, Is.EqualTo(3));
        }
    }
}