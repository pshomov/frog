using System;
using System.Linq;
using KellermanSoftware.CompareNetObjects;
using NUnit.Framework;

namespace Frog.Domain.Specs.PipelineStatusViewSpecs
{
    [TestFixture]
    public class StatusViewAfterBuildUpdateSpec : StatusViewAfterBuildStarterSpecBase
    {
        protected override void Given()
        {
            base.Given();
            var pipelineStatus = new PipelineStatus(Guid.NewGuid())
            {
                Tasks =
                                         {
                                             new TaskInfo
                                                 {Name = "task1", Status = TaskInfo.TaskStatus.NotStarted}
                                         }
            };

            View.Handle(new BuildStarted("http://dugh", pipelineStatus));
        }

        protected override void When()
        {
            PipelineStatus = new PipelineStatus(Guid.NewGuid())
                                 {
                                     Tasks =
                                         {
                                             new TaskInfo
                                                 {Name = "task1", Status = TaskInfo.TaskStatus.Started}
                                         }
                                 };
            View.Handle(new BuildUpdated("http://dugh", new PipelineStatus(PipelineStatus)));
        }

        [Test]
        public void should_not_modify_build_status()
        {
            Assert.That(BuildStatuses["http://dugh"].Current,
                        Is.EqualTo(UI.PipelineStatusView.BuildStatus.OverallStatus.NotStarted));
        }

        [Test]
        public void should_set_task_status_as_as_in_message()
        {
            var comparator = new CompareObjects();
            var taskStates = BuildStatuses["http://dugh"].TaskState.ToList();
            var equalItems = PipelineStatus.Tasks.Where(
                (state, i) => comparator.Compare(state.Status, taskStates[i]));
            Assert.That(equalItems.Count(), Is.EqualTo(PipelineStatus.Tasks.Count));
        }
    }
}