using System;
using System.Linq;
using Frog.Domain;
using Frog.Domain.Integration;
using Frog.Domain.UI;
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

        [SetUp]
        public void SetUp()
        {
            theBus = new RabbitMQBus(OSHelpers.RabbitHost());

            views = new PersistentProjectView(OSHelpers.RiakHost(), OSHelpers.RiakPort(), "loadtest_buildsIds", "loadtest_reposBucket");
            views.WipeBucket();
            views2 = new PersistentProjectView(OSHelpers.RiakHost(), OSHelpers.RiakPort(), "loadtest_buildsIds", "loadtest_reposBucket");
            Setup.SetupView(theBus, views);
        }
        [Test]
        public void should_process_1000_messages()
        {

            var buildId = Guid.NewGuid();
            const string repoUrl = "http://github.com/never/neverland.git";
            theBus.Publish(new ProjectCheckedOut(){BuildId = buildId, CheckoutInfo = new CheckoutInfo(){Comment = "asd", Revision = "12"}, RepoUrl = repoUrl});
            theBus.Publish(new BuildStarted()
                               {
                                   BuildId = buildId,
                                   Status = new PipelineStatus() {Tasks = As.List(new TaskInfo("t1"))},
                                   RepoUrl = repoUrl
                               });
            theBus.Publish(new BuildUpdated(){BuildId = buildId, TaskIndex = 0, TaskStatus = TaskInfo.TaskStatus.Started});
            Enumerable.Range(0,1000).ToList().ForEach(i => theBus.Publish(new TerminalUpdate(){BuildId = buildId, Content = "content", ContentSequenceIndex = i, TaskIndex = 0}));
            theBus.Publish(new BuildUpdated(){BuildId = buildId, TaskIndex = 0, TaskStatus = TaskInfo.TaskStatus.FinishedSuccess});
            theBus.Publish(new BuildEnded(){BuildId = buildId, TotalStatus = BuildTotalEndStatus.Success});

            Specs.Support.AssertionHelpers.WithRetries(() =>
                                                                {
                                                                    try
                                                                    {
                                                                        Assert.That(views2.GetBuildStatus(buildId).Overall,
                                                                                    Is.EqualTo(
                                                                                        BuildTotalStatus.BuildEndedSuccess));
                                                                    }
                                                                    catch (BuildNotFoundException e)
                                                                    {
                                                                        throw new AssertionException("build not found");
                                                                    }
                                                                }, 1000);
        }

    }
}
