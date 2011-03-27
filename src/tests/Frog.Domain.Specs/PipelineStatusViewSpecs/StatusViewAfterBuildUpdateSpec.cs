using System;
using KellermanSoftware.CompareNetObjects;
using NUnit.Framework;

namespace Frog.Domain.Specs.PipelineStatusViewSpecs
{
    [TestFixture]
    public class StatusViewAfterBuildUpdateSpec : StatusViewAfterBuildStarterSpecBase
    {
        protected override void When()
        {
            PipelineStatus = new PipelineStatus(Guid.NewGuid())
                                 {
                                     Tasks =
                                         {
                                             new TasksInfo
                                                 {Name = "task1", Status = TasksInfo.TaskStatus.Started}
                                         }
                                 };
            View.Handle(new BuildUpdated("http://dugh", new PipelineStatus(PipelineStatus)));
        }

        [Test]
        public void should_not_modify_build_status()
        {
            Assert.That(BuildStatuses["http://dugh"].Current,
                        Is.EqualTo(UI.PipelineStatusView.BuildStatus.Status.NotStarted));
        }

        [Test]
        public void should_set_task_status_to_the_value_sent_in_the_event()
        {
            Assert.True(new CompareObjects().Compare(BuildStatuses["http://dugh"].PipelineStatus, PipelineStatus));
        }
    }
}