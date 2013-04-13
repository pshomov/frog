using System;
using Frog.Domain;
using Frog.Specs.Support;
using Frog.System.Specs.Underware;
using NSubstitute;
using NUnit.Framework;
using xray;

namespace Frog.System.Specs.GetLatestRevision
{
    [TestFixture]
    public class CheckingForUpdatesTwiceInARowWithErrorResponseInBetween : BDD
    {
        SystemDriver system;
        private PollingProber prober;

        protected override void Given()
        {
            var source_repo_driver = Substitute.For<SourceRepoDriver>();
            source_repo_driver.GetLatestRevision().Returns(info =>
            {
                throw new NullReferenceException("fake one");
            });

            var testSystem = new TestSystem().
                WithRepositoryTracker().
                WithRevisionChecker(url => source_repo_driver);
            system = new SystemDriver(testSystem);

            system.RegisterNewProject("http://123");
            system.CheckProjectsForUpdates();
            prober = new PollingProber(5000, 100);
            Assert.True(prober.check(Take.Snapshot(() => system.GetEventsSnapshot())
                                         .Has(msgs => msgs, An.Event<CheckForUpdateFailed>())
                                         .Has(msgs => msgs, A.Command<CheckRevision>())
                            ));
        }

        protected override void When()
        {
            system.CheckProjectsForUpdates();
        }

        protected override void GivenCleanup()
        {
            system.Stop();
        }

        [Test]
        public void should_send_CHECK_FOR_UPDATE_command_twice()
        {
            Assert.True(prober.check(Take.Snapshot(() => system.GetEventsSnapshot())
                                         .Has(x => x,
                                              Two.Commands<CheckRevision>())
                            ));
        }
    }
}