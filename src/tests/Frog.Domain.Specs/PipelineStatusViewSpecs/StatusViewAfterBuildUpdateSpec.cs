﻿using System;
using System.Linq;
using Frog.Domain.UI;
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

            View.Handle(new BuildStarted(BuildMessage.BuildId, pipelineStatus, "http://"));
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
            View.Handle(new BuildUpdated(BuildMessage.BuildId, 0, TaskInfo.TaskStatus.FinishedError));
        }

        [Test]
        public void should_not_modify_build_status()
        {
            Assert.That(BuildStatuses[BuildMessage.BuildId].Overall,
                        Is.EqualTo(BuildTotalStatus.BuildStarted));
        }

        [Test]
        public void should_set_task_status_as_as_in_message()
        {
            Assert.That(BuildStatuses[BuildMessage.BuildId].Tasks[0].Status,
                        Is.EqualTo(TaskInfo.TaskStatus.FinishedError));
        }
    }
}