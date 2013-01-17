using System;
using System.Linq;
using EventStore;
using Frog.Domain;
using Frog.Domain.Integration;
using Frog.Domain.Integration.UI;
using Frog.Specs.Support;
using Frog.Support;
using NUnit.Framework;
using SimpleCQRS;

namespace Frog.LoadTests
{
    [TestFixture]
    public class BuildRunEventProcessing : BDD
    {
        const int TERMINAL_UPDATES_COUNT = 300;
        private ProjectView views;
        private IBus theBus;

        private IStoreEvents eventStore;
        BuildView build_view;
        Guid build_id;
        ViewForTerminalOutput terminal_outptut;
        Guid terminal_id;


        protected override void GivenCleanup()
        {
            if (Profiler.MeasurementsBridge != null)
            {
                Profiler.MeasurementsBridge.Dispose();
                Profiler.MeasurementsBridge = null;
            }
        }

        protected override void Given()
        {
            eventStore = Config.WireupEventStore();
            eventStore.Advanced.Purge();
            eventStore.Advanced.Initialize();
            Profiler.MeasurementsBridge = new Profiler.LogFileLoggingBridge("load_tests.log");
            views = new EventBasedProjectView(eventStore);
            build_view = (BuildView)views;
            ((ProjectTestSupport)views).WipeBucket();
            terminal_outptut = new EventBasedViewForTerminalOutput(eventStore);
            theBus = new RabbitMQBus(OSHelpers.RabbitHost());
            SetupHandlers(5);
        }

        void SetupHandlers(int howManyInParallel)
        {
            for (int i = 0; i < howManyInParallel; i++)
            {
                var wireupEventStore = Config.WireupEventStore();
                var bus = new RabbitMQBus(OSHelpers.RabbitHost());
                Domain.Integration.UI.Setup.SetupView(bus, wireupEventStore);
            }
        }

        protected override void When()
        {
            build_id = Guid.NewGuid();
            const string repoUrl = "http://github.com/never/neverland.git";
            theBus.Publish(new ProjectCheckedOut(build_id, 0) { CheckoutInfo = new CheckoutInfo { Comment = "asd", Revision = "12" }, RepoUrl = repoUrl });
            theBus.Publish(new BuildStarted(build_id, new PipelineStatus { Tasks = As.List(new TaskInfo("t1", new Guid())) }, repoUrl, 0
                               ));
            terminal_id = Guid.NewGuid();
            theBus.Publish(new BuildUpdated(build_id, 0, TaskInfo.TaskStatus.Started, 1, terminal_id));
            Enumerable.Range(0, TERMINAL_UPDATES_COUNT).ToList().ForEach(i => theBus.Publish(new TerminalUpdate(content: "content", taskIndex: 0, contentSequenceIndex: i, buildId: build_id, sequenceId: i + 2, terminalId: terminal_id)));
            theBus.Publish(new TerminalUpdate(content: "what?", taskIndex: 0, contentSequenceIndex: TERMINAL_UPDATES_COUNT, buildId: build_id, sequenceId: TERMINAL_UPDATES_COUNT + 2, terminalId: terminal_id));
            theBus.Publish(new BuildUpdated(build_id, 0, TaskInfo.TaskStatus.FinishedSuccess, TERMINAL_UPDATES_COUNT+3, terminal_id));
            theBus.Publish(new BuildEnded(build_id, BuildTotalEndStatus.Success, TERMINAL_UPDATES_COUNT+4));
        }

        [Test]
        public void should_get_build_status_done()
        {

            AssertionHelpers.WithRetries(() =>
                                             {
                                                 try
                                                 {
                                                     Assert.That(build_view.GetBuildStatus(build_id).Overall,
                                                                 Is.EqualTo(
                                                                     BuildTotalStatus.BuildEndedSuccess));
                                                 }
                                                 catch (BuildNotFoundException)
                                                 {
                                                     throw new AssertionException("build not found");
                                                 }
                                             }, 1000);
        }
        [Test]
        public void should_finish_processing_terminal_output()
        {
            AssertionHelpers.WithRetries(() => Assert.That(terminal_outptut.GetTerminalOutput(terminal_id), Is.StringEnding("what?")), 1000);
        }
    }
}
