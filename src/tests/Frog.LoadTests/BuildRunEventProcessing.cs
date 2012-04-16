using System;
using System.Linq;
using EventStore;
using Frog.Domain;
using Frog.Domain.Integration;
using Frog.Domain.UI;
using Frog.Specs.Support;
using Frog.Support;
using NUnit.Framework;
using SimpleCQRS;

namespace Frog.LoadTests
{
    [TestFixture]
    public class BuildRunEventProcessing
    {
        private ProjectView views;
        private IBus theBus;
        private ProjectView views2;

        private IStoreEvents eventStore;
        BuildView build_view;

        [SetUp]
        public void SetUp()
        {
            eventStore = StoreFactory.WireupEventStore();
            eventStore.Advanced.Purge();
            eventStore.Advanced.Initialize();
            Profiler.MeasurementsBridge = new Profiler.LogFileLoggingBridge("system_tests.log");
            theBus = new RabbitMQBus(OSHelpers.RabbitHost());

            views = new EventBasedProjectView(eventStore);
            build_view = (BuildView) views;
            ((ProjectTestSupport) views).WipeBucket();
            views2 = new EventBasedProjectView(eventStore);
            Setup.SetupView(theBus, eventStore);
        }

        [TearDown]
        public void TearDown()
        {
            Profiler.MeasurementsBridge.Dispose();
            Profiler.MeasurementsBridge = null;
        }

        [Test]
        public void should_process_1000_messages()
        {

            var buildId = Guid.NewGuid();
            const string repoUrl = "http://github.com/never/neverland.git";
            theBus.Publish(new ProjectCheckedOut(buildId,0){CheckoutInfo = new CheckoutInfo(){Comment = "asd", Revision = "12"}, RepoUrl = repoUrl});
            theBus.Publish(new BuildStarted(buildId, new PipelineStatus() {Tasks = As.List(new TaskInfo("t1", new Guid()))}, repoUrl,0
                               ));
            var terminalId = Guid.NewGuid();
            theBus.Publish(new BuildUpdated(buildId, 0, TaskInfo.TaskStatus.Started, 1, terminalId));
            Enumerable.Range(0,1000).ToList().ForEach(i => theBus.Publish(new TerminalUpdate(content : "content", taskIndex : 0,contentSequenceIndex : i, buildId : buildId, sequenceId:i+2, terminalId: terminalId)));
            theBus.Publish(new BuildUpdated(buildId, 0, TaskInfo.TaskStatus.FinishedSuccess, 1002, terminalId));
            theBus.Publish(new BuildEnded(buildId, BuildTotalEndStatus.Success, 2003));

            AssertionHelpers.WithRetries(() =>
                                                                {
                                                                    try
                                                                    {
                                                                        Assert.That(build_view.GetBuildStatus(buildId).Overall,
                                                                                    Is.EqualTo(
                                                                                        BuildTotalStatus.BuildEndedSuccess));
                                                                    }
                                                                    catch (BuildNotFoundException)
                                                                    {
                                                                        throw new AssertionException("build not found");
                                                                    }
                                                                }, 1000);
        }

    }
}
