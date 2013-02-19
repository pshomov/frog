using System;
using System.IO;
using Frog.Domain.BuildSystems.Make;
using Frog.Domain.BuildSystems.Rake;
using Frog.Domain.BuildSystems.Solution;
using Frog.Specs.Support;
using Frog.Support;
using NSubstitute;
using NUnit.Framework;

namespace Frog.Domain.Specs
{
    [TestFixture]
    public class TaskGeneratorXBuildTasksSpec : BDD
    {
        ExecTaskGenerator execTaskGenerator;
        ExecTaskFactory execTaskFactory;

        protected override void Given()
        {
            execTaskFactory = Substitute.For<ExecTaskFactory>();
            execTaskGenerator = new ExecTaskGenerator(execTaskFactory, OS.Unix);
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
    public class TaskGeneratorMSBuildTasksSpec : BDD
    {
        ExecTaskGenerator execTaskGenerator;
        ExecTaskFactory execTaskFactory;

        protected override void Given()
        {
            execTaskFactory = Substitute.For<ExecTaskFactory>();
            execTaskGenerator = new ExecTaskGenerator(execTaskFactory, OS.Windows);
        }

        protected override void When()
        {
            execTaskGenerator.GimeTasks(new MSBuildTask("fle.sln"));
        }

        [Test]
        public void should_have_msbuild_task()
        {
            execTaskFactory.Received().CreateTask("{0}\\Microsoft.NET\\Framework\\v4.0.30319\\msbuild.exe".Formatt(Environment.GetEnvironmentVariable("SYSTEMROOT")), Arg.Any<string>(), "build");
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
            execTaskGenerator = new ExecTaskGenerator(execTaskFactory, OS.Unix);
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
            execTaskGenerator = new ExecTaskGenerator(execTaskFactory, OS.Unix);
        }

        protected override void When()
        {
            execTaskGenerator.GimeTasks(new RakeTask());
        }

        [Test]
        public void should_create_a_task_that_launches_Rake()
        {
            execTaskFactory.Received().CreateTask(Arg.Any<string>(), Arg.Is<string>(s => s.Contains("rake")), Arg.Any<string>());
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
            execTaskGenerator = new ExecTaskGenerator(execTaskFactory, OS.Unix);
        }

        protected override void When()
        {
            execTaskGenerator.GimeTasks(new BundlerTask());
        }

        [Test]
        public void should_have_bundle_task()
        {
            execTaskFactory.Received().CreateTask(Arg.Any<string>(), Arg.Is<string>(s => s.Contains("bundle install --path ~/.gem")), Arg.Any<string>());
        }
    }

    [TestFixture]
    public class TaskGeneratorMakeTaskSpec : BDD
    {
        ExecTaskGenerator execTaskGenerator;
        ExecTaskFactory execTaskFactory;

        protected override void Given()
        {
            execTaskFactory = Substitute.For<ExecTaskFactory>();
            execTaskGenerator = new ExecTaskGenerator(execTaskFactory, OS.Unix);
        }

        protected override void When()
        {
            execTaskGenerator.GimeTasks(new MakeTask());
        }

        [Test]
        public void should_have_make_task()
        {
            execTaskFactory.Received().CreateTask(Arg.Is<string>(s => s == "make"), Arg.Is((string)null), "Make task");
        }
    }
}