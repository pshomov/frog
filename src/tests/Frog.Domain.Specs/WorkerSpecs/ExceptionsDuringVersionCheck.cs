using System;
using Frog.Specs.Support;
using NSubstitute;
using NUnit.Framework;

namespace Frog.Domain.Specs.WorkerSpecs
{
    [TestFixture]
    public class ExceptionsDuringVersionCheck : BDD
    {
        int eventCount;
        private RevisionChecker.RevisionChecker worker;

        protected override void Given()
        {
            eventCount = 0;
            var sourceRepoDriver = Substitute.For<SourceRepoDriver>();
            sourceRepoDriver.GetLatestRevision().Returns(info => {throw new NullReferenceException();});

            worker = new RevisionChecker.RevisionChecker(p);
            worker.OnCheckForUpdateFailed  += delegate { eventCount++; };
        }

        protected override void When()
        {
            worker.RevisionCheck("http://repo");
        }

        [Test]
        public void should_have_reised_the_event_subscribers_ony_for_the_current_check()
        {
            Assert.That(eventCount, Is.EqualTo(1));
        }
    }
}