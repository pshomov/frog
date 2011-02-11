using System;
using System.Collections;
using Frog.Domain.CustomTasks;
using Frog.Domain.TaskDetection;
using Frog.Support;
using NSubstitute;
using NUnit.Framework;

namespace Frog.Domain.Specs
{
    [TestFixture]
    public class ExecTaskGeneratorSpec : BDD
    {
        ExecTaskGenerator execTaskGenerator;
        ExecTaskFactory execTaskFactory;

        public override void Given()
        {
            execTaskFactory = Substitute.For<ExecTaskFactory>();
            var taskSource = Substitute.For<TaskSource>();
            taskSource.Detect("flee").Returns(As.List<ITask>(new MSBuildTaskDescriptions("fle.sln"), new NUnitTask("flo.test.dll")));
            execTaskGenerator = new ExecTaskGenerator(taskSource, execTaskFactory);
        }

        public override void When()
        {
            execTaskGenerator.GimeTasks("flee");
        }

        [Test]
        public void should_have_xbuild_task()
        {
            execTaskFactory.Received().CreateTask("xbuild", Arg.Any<string>(), "build");
        }

        [Test]
        public void should_have_nunit_task()
        {
            execTaskFactory.Received().CreateTask("nunit", Arg.Any<string>(), "unit_test");
        }
    }
}