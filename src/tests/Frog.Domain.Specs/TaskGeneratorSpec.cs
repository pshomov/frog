using Frog.Domain.CustomTasks;
using Frog.Specs.Support;
using NSubstitute;
using NUnit.Framework;

namespace Frog.Domain.Specs
{
    [TestFixture]
    public class TaskGeneratorMSBuildTasksSpec : BDD
    {
        ExecTaskGenerator execTaskGenerator;
        ExecTaskFactory execTaskFactory;

        protected override void Given()
        {
            execTaskFactory = Substitute.For<ExecTaskFactory>();
            execTaskGenerator = new ExecTaskGenerator(execTaskFactory);
        }

        protected override void When()
        {
            execTaskGenerator.GimeTasks(new MSBuildTask("fle.sln"));
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

        protected override void Given()
        {
            execTaskFactory = Substitute.For<ExecTaskFactory>();
            execTaskGenerator = new ExecTaskGenerator(execTaskFactory);
        }

        protected override void When()
        {
            execTaskGenerator.GimeTasks(new NUnitTask("fle.csproj"));
        }

        [Test]
        public void should_have_nunit_task()
        {
            execTaskFactory.Received().CreateTask("nunit", Arg.Any<string>(), "unit_test");
        }
    }

    [TestFixture]
    public class TaskGeneratorRakeTasksSpec : BDD
    {
        ExecTaskGenerator execTaskGenerator;
        ExecTaskFactory execTaskFactory;

        protected override void Given()
        {
            execTaskFactory = Substitute.For<ExecTaskFactory>();
            execTaskGenerator = new ExecTaskGenerator(execTaskFactory);
        }

        protected override void When()
        {
            execTaskGenerator.GimeTasks(new RakeTask());
        }

        [Test]
        public void should_have_rake_task()
        {
            execTaskFactory.Received().CreateTask("rake", Arg.Any<string>(), "unit_test");
        }
    }

    [TestFixture]
    public class TaskGeneratorBundlerTasksSpec : BDD
    {
        ExecTaskGenerator execTaskGenerator;
        ExecTaskFactory execTaskFactory;

        protected override void Given()
        {
            execTaskFactory = Substitute.For<ExecTaskFactory>();
            execTaskGenerator = new ExecTaskGenerator(execTaskFactory);
        }

        protected override void When()
        {
            execTaskGenerator.GimeTasks(new BundlerTask());
        }

        [Test]
        public void should_have_bundle_task()
        {
            execTaskFactory.Received().CreateTask("bundle", Arg.Any<string>(), "Bundler");
        }
    }
}