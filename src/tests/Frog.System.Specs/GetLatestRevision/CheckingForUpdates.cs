using System.Linq;
using Frog.Domain;
using Frog.System.Specs.Underware;
using NSubstitute;
using NUnit.Framework;
using xray;

namespace Frog.System.Specs.GetLatestRevision
{
    [TestFixture]
    public class CheckingForUpdates : SystemBDD
    {
        protected override void Given()
        {
            base.Given();
            var source_repo_driver = Substitute.For<SourceRepoDriver>();
            source_repo_driver.GetLatestRevision().Returns(new RevisionInfo {Revision = "12"});
            testSystem
                .WithRepositoryTracker()
                .WithRevisionChecker(url => source_repo_driver);
            system.RegisterNewProject("http://123");
        }

        protected override void When()
        {
            system.CheckProjectsForUpdates();
        }

        [Test]
        public void should_check_for_new_revision_and_request_a_build_of_it()
        {
            Assert.True(EventStoreCheck(ES =>
                                        ES.Has(
                                            A.Command<CheckRevision>(
                                                ev =>
                                                ev.RepoUrl == "http://123"))
                                          .Has(
                                              An.Event<UpdateFound>(
                                                  found =>
                                                  found.RepoUrl == "http://123" && found.Revision.Revision == "12"))
                                          .Has(
                                              A.Command<BuildRequest>(
                                                  found =>
                                                  found.RepoUrl == "http://123" && found.Revision.Revision == "12"))
                            ));
        }

        [Test]
        public void should_send_UPDATE_FOUND_message_and_update_the_last_build_revision()
        {
            var messageCheckpoint = system.GetEventsSnapshot().Count;
            system.CheckProjectsForUpdates();
            Assert.True(EventStoreCheck(ES =>
                                        ES.Has(x => x.Skip(messageCheckpoint).ToList(),
                                               A.Command<CheckRevision>(
                                                   ev =>
                                                   ev.RepoUrl == "http://123"))
                            ));
            Assert.False(EventStoreCheck(ES =>
                                         ES.Has(x => x.Skip(messageCheckpoint).ToList(),
                                                A.Command<BuildRequest>(
                                                    ev =>
                                                    ev.RepoUrl == "http://123"))
                             ));
        }
    }
}