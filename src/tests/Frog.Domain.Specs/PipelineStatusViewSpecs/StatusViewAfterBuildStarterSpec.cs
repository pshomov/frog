﻿using System;
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
            Assert.That(BuildStatuses["http://repo"].Overall,
                        Is.EqualTo(BuildTotalStatus.BuildStarted));
        }
    }
}