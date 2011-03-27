using System;
using KellermanSoftware.CompareNetObjects;
using NUnit.Framework;

namespace Frog.Domain.Specs.PipelineStatusView
{
    [TestFixture]
    public class StatusViewAfterBuildUpdateSpec : StatusViewAfterBuildStarterSpecBase
    {
        protected override void When()
        {
            pipelineStatus = new PipelineStatus(Guid.NewGuid())
                                 {
                                     Tasks =
                                         {
                                             new TasksInfo
                                                 {Name = "task1", Status = TasksInfo.TaskStatus.Started}
                                         }
                                 };
            view.Handle(new BuildUpdated("http://dugh", new PipelineStatus(pipelineStatus)));
        }

        [Test]
        public void should_not_modify_build_status()
        {
            Assert.That(buildStatuses["http://dugh"].Current,
                        Is.EqualTo(UI.PipelineStatusView.BuildStatus.Status.NotStarted));
        }

        [Test]
        public void should_set_task_status_to_the_value_sent_in_the_event()
        {
            Assert.True(new CompareObjects().Compare(buildStatuses["http://dugh"].PipelineStatus, pipelineStatus));
        }
    }
}