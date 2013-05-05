using System;
using Frog.Domain;
using Frog.System.Specs.Underware;
using NSubstitute;
using NUnit.Framework;
using xray;

namespace Frog.System.Specs.GetLatestRevision
{
    [TestFixture]
    public class CheckingForUpdatesTwiceInARowWhileAgentIsNotRunning : SystemBDD
    {
        protected override void Given()
        {
            base.Given();
            var repoUrl = Guid.NewGuid().ToString();
            var source_repo_driver = Substitute.For<SourceRepoDriver>();
            source_repo_driver.GetLatestRevision().Returns(new RevisionInfo { Revision = "14" });

            testSystem
                .WithRepositoryTracker();

            system.RegisterNewProject(repoUrl);
            system.CheckProjectsForUpdates();
            make_sure_one_CHECK_FOR_UPDATE_message_is_sent();
        }

        protected override void When()
        {
            system.CheckProjectsForUpdates();
        }

        [Test]
        public void should_send_CHECK_FOR_UPDATE_command_only_once()
        {
            Assert.True(EventStoreCheck(ES =>
                                         ES.Has(A.Command<CheckRevision>())
                            ));
            Assert.False(EventStoreCheck(ES =>
                                         ES.Has(Two.Commands<CheckRevision>())
                            ));
        }

        private void make_sure_one_CHECK_FOR_UPDATE_message_is_sent()
        {
            Assert.True(EventStoreCheck(ES =>
                                         ES.Has(A.Command<CheckRevision>())
                            ));
        }
    }
}