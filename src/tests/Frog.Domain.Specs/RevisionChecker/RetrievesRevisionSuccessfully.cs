﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Frog.Domain.RevisionChecker;
using Frog.Specs.Support;
using NSubstitute;
using NUnit.Framework;
using SimpleCQRS;

namespace Frog.Domain.Specs.RevisionChecker
{
    class RetrievesRevisionSuccessfully : BDD
    {
        private Domain.RevisionChecker.RevisionChecker rc;
        private SourceRepoDriver sr;
        private IBus bus;

        protected override void Given()
        {
            sr = Substitute.For<SourceRepoDriver>();
            sr.GetLatestRevision().Returns("456");
            bus = NSubstitute.Substitute.For<IBus>();
            rc = new Frog.Domain.RevisionChecker.RevisionChecker(bus, url => sr);
        }

        protected override void When()
        {
            rc.Handle(new CheckRevision {RepoUrl = "http://fle"});
        }

        [Test]
        public void should_get_revision_number()
        {
            sr.Received().GetLatestRevision();
        }

        [Test]
        public void should_send_event_with_revision_found()
        {
            bus.Received().Publish(Arg.Is<UpdateFound>(found => found.RepoUrl == "http://fle" && found.Revision == "456"));
        }
    }
}