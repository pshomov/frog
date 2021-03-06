﻿using System;
using Frog.Specs.Support;
using NSubstitute;
using NUnit.Framework;
using SimpleCQRS;

namespace Frog.Domain.Specs.RevisionChecker
{
    public class FailsToRetrieveRevision : BDD
    {
        private Domain.RevisionChecker rc;
        private SourceRepoDriver sr;
        private IBus bus;

        protected override void Given()
        {
            sr = Substitute.For<SourceRepoDriver>();
            sr.When(driver => driver.GetLatestRevision()).Do(info => {throw new TimeoutException();});
            bus = Substitute.For<IBus>();
            rc = new Domain.RevisionChecker(bus, url => sr);
        }

        protected override void When()
        {
            rc.Handle(new CheckRevision {RepoUrl = "http://fle"});
        }

        [Test]
        public void should_send_event_with_revision_found()
        {
            bus.Received().Publish(Arg.Is<CheckForUpdateFailed>(found => found.RepoUrl == "http://fle"));
        }
    }
}
