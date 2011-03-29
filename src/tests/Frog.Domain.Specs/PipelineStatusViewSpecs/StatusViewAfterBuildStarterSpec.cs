using System;
using System.Collections.Generic;
using System.Linq;
using Frog.Domain.UI;
using KellermanSoftware.CompareNetObjects;
using NUnit.Framework;

namespace Frog.Domain.Specs.PipelineStatusViewSpecs
{
    [TestFixture]
    public class StatusViewAfterBuildStarterSpec : StatusViewAfterBuildStarterSpecBase
    {
        protected override void When()
        {
            PipelineStatus = new PipelineStatus(Guid.NewGuid())
                                 {
                                     Tasks =
                                         {
                                             new TaskInfo
                                                 {Name = "task1", Status = TaskInfo.TaskStatus.NotStarted}
                                         }
                                 };
            View.Handle(new BuildStarted("http://repo", new PipelineStatus(PipelineStatus)));
        }

        [Test]
        public void should_set_status_to_BUILD_STARTED()
        {
            Assert.That(BuildStatuses["http://repo"].Current,
                        Is.EqualTo(PipelineStatusView.BuildStatus.OverallStatus.PipelineStarted));
        }

        [Test]
        public void should_set_task_status_as_as_in_message()
        {
            var comparator = new CompareObjects();
            var taskStates = BuildStatuses["http://repo"].TaskState.ToList();
            var equalItems = taskStates.Where(
                (state, i) => comparator.Compare(state.Status, PipelineStatus.Tasks[i]));
            Assert.That(equalItems.Count(), Is.EqualTo(taskStates.Count));
        }
    }
}