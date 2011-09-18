using Frog.Domain.RepositoryTracker;
using Frog.Domain.UI;
using NUnit.Framework;

namespace Frog.Domain.Specs.PipelineStatusViewSpecs
{
    [TestFixture]
    public class StatusViewAfterBuildCompletedWithErrorSpec : StatusViewAfterBuildStarterSpecBase
    {
        protected override void When()
        {
            View.Handle(new BuildEnded(BuildMessage.BuildId, BuildTotalEndStatus.Error));
        }

        [Test]
        public void should_set_status_to_BUILD_COMPLETED()
        {
            Assert.That(BuildStatuses[BuildMessage.BuildId].Overall,
                        Is.EqualTo(BuildTotalStatus.BuildEndedError));
        }
    }
}