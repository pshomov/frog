using System;
using Frog.Domain;
using Frog.System.Specs.Underware;
using NSubstitute;
using NUnit.Framework;
using xray;

namespace Frog.System.Specs.GetLatestRevision
{
    [TestFixture]
    public class CheckingForUpdatesTwiceInARowWithErrorResponseInBetween : SystemBDD
    {

        protected override void Given()
        {
            base.Given();
            var source_repo_driver = Substitute.For<SourceRepoDriver>();
            source_repo_driver.GetLatestRevision().Returns(info =>
            {
                throw new NullReferenceException("fake one");
            });

            testSystem
                .WithRepositoryTracker()
                .WithRevisionChecker(url => source_repo_driver);

            system.RegisterNewProject("http://123");
            system.CheckProjectsForUpdates();
            Assert.True(EventStoreCheck(ES => ES
                                         .Has(An.Event<CheckForUpdateFailed>())
                                         .Has(A.Command<CheckRevision>())
                            ));
        }

        protected override void When()
        {
            system.CheckProjectsForUpdates();
        }

        [Test]
        public void should_send_CHECK_FOR_UPDATE_command_twice()
        {
            Assert.True(EventStoreCheck(ES => ES
                                         .Has(Two.Commands<CheckRevision>())
                            ));
        }
    }
}