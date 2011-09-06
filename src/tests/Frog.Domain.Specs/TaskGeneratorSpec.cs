using System.IO;
using Frog.Domain.BuildSystems.Rake;
using Frog.Domain.BuildSystems.Solution;
using Frog.Specs.Support;
using Frog.Support;
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
            execTaskFactory.Received().CreateTask(Arg.Any<string>(), Arg.Is<string>(s => s.Contains("bundle exec rake")), Arg.Any<string>());
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
            execTaskFactory.Received().CreateTask(Arg.Any<string>(), Arg.Is<string>(s => s.Contains("bundle install --path runz-built-deps")), Arg.Any<string>());
        }
    }

    [TestFixture]
    public class TaskGeneratorShellTasksSpec : BDD
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
            execTaskGenerator.GimeTasks(new ShellTask(){cmd = "ccc", args = "/a /b"});
        }

        [Test]
        public void should_have_bundle_task()
        {
            execTaskFactory.Received().CreateTask(Arg.Is<string>(s => s == "cmd.exe" || s == "/bin/bash"), Arg.Is<string>(s => s == "-c \"ccc /a /b\"" || s == "/c ccc /a /b"), "Shell Task");
        }
    }
}