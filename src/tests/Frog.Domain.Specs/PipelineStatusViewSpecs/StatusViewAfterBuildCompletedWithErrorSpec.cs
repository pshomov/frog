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
            View.Handle(new BuildEnded(BuildMessage.Id, BuildTotalEndStatus.Error));
        }

        [Test]
        public void should_set_status_to_BUILD_COMPLETED()
        {
            Assert.That(BuildStatuses[BuildMessage.Id].Overall,
                        Is.EqualTo(BuildTotalStatus.BuildEndedError));
        }
    }
}