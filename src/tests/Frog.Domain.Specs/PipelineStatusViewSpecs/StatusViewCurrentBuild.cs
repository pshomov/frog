using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Frog.Domain.RepositoryTracker;
using Frog.Domain.UI;
using Frog.Specs.Support;
using NUnit.Framework;

namespace Frog.Domain.Specs.PipelineStatusViewSpecs
{
    class StatusViewCurrentBuild : BDD
    {
        private const string RepoUrl = "http://lilalo";
        protected PipelineStatusView View;
        protected PipelineStatus PipelineStatus;
        protected ConcurrentDictionary<Guid, BuildStatus> BuildStatuses;
        protected Build BuildMessage;
        private ConcurrentDictionary<string, Guid> currentBuilds;

        protected override void Given()
        {
            BuildStatuses = new ConcurrentDictionary<Guid, BuildStatus>();
            currentBuilds = new ConcurrentDictionary<string, Guid>();
            View = new PipelineStatusView(BuildStatuses, currentBuilds);

        }

        protected override void When()
        {
            BuildMessage = new Build{RepoUrl = RepoUrl, Revision = "23"};
            View.Handle(BuildMessage);
        }

        [Test]
        public void should_have_the_new_buildId_as_the_current_build()
        {
            Assert.That(currentBuilds[RepoUrl], Is.EqualTo(BuildMessage.Id));
        }

    }
}
