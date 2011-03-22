using Frog.Domain.CustomTasks;
using NSubstitute;
using NUnit.Framework;

namespace Frog.Domain.Specs
{
    [TestFixture]
    public class TaskGeneratorMSBuildTasksSpec : BDD
    {
        ExecTaskGenerator execTaskGenerator;
        ExecTaskFactory execTaskFactory;

        public override void Given()
        {
            execTaskFactory = Substitute.For<ExecTaskFactory>();
            execTaskGenerator = new ExecTaskGenerator(execTaskFactory);
        }

        public override void When()
        {
            execTaskGenerator.GimeTasks(new MSBuildTaskDescriptions("fle.sln"));
        }

        [Test]
        public void should_have_xbuild_task()
        {
            execTaskFactory.Received().CreateTask("xbuild", Arg.Any<string>(), "build");
        }
    }

    [TestFixture]
    public class TaskGeneratorNUnitTasksSpec : BDD
    {
        ExecTaskGenerator execTaskGenerator;
        ExecTaskFactory execTaskFactory;

        public override void Given()
        {
            execTaskFactory = Substitute.For<ExecTaskFactory>();
            execTaskGenerator = new ExecTaskGenerator(execTaskFactory);
        }

        public override void When()
        {
            execTaskGenerator.GimeTasks(new NUnitTask("fle.csproj"));
        }

        [Test]
        public void should_have_nunit_task()
        {
            execTaskFactory.Received().CreateTask("nunit", Arg.Any<string>(), "unit_test");
        }
    }


}