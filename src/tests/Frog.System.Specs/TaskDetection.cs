using Frog.Domain;
using Frog.Domain.CustomTasks;
using Frog.Domain.Specs;
using Frog.Domain.TaskSources;
using Frog.Support;
using NSubstitute;
using NUnit.Framework;

namespace Frog.System.Specs
{
    [TestFixture]
    public class TaskDetection : BDD
    {
        Valve valve;
        PipelineOfTasks pipeline;
        SystemDriver system;
        ExecTaskFactory execTaskFactory;
        RepositoryDriver repo;

        public override void Given()
        {
            system = SystemDriver.GetCleanSystem();
            repo = RepositoryDriver.GetNewRepository();

            var execTask = Substitute.For<ExecTask>("", "", "");
            execTask.Perform(Arg.Any<SourceDrop>()).Returns(new ExecTaskResult(ExecTask.ExecutionStatus.Success, 0));

            execTaskFactory = Substitute.For<ExecTaskFactory>();
            execTaskFactory.CreateTask(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>()).Returns(execTask);

            var fileFinder = new DefaultFileFinder(new PathFinder());
            pipeline = new PipelineOfTasks(system.Bus,
                                           new CompoundTaskSource(new MSBuildDetector(fileFinder),
                                                                  new NUnitTaskDetctor(fileFinder)),
                                           new ExecTaskGenerator(execTaskFactory));
            system.MonitorRepository(repo.Url);
            valve = new Valve(system.Git, pipeline, system.WorkingArea);
        }

        public override void When()
        {
            repo.CommitDirectoryTree("TestFixture");
            valve.Check();
        }

        [Test]
        public void should_have_build_result_in_build_complete_event()
        {
            execTaskFactory.Received().CreateTask("nunit", Os.DirChars("src/tests/Some.Tests/bin/Debug/Some.Test.dll"),
                                                  Arg.Any<string>());
        }

        [Test]
        public void should_execute_xbuild_task()
        {
            execTaskFactory.Received().CreateTask("xbuild", Os.DirChars("SampleProject.sln"), Arg.Any<string>());
        }

        protected override void GivenCleanup()
        {
            system.ResetSystem();
            repo.Cleanup();
        }
    }
}