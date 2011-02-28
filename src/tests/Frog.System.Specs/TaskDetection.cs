using Frog.Domain;
using Frog.Domain.CustomTasks;
using Frog.Domain.Specs;
using Frog.Domain.TaskSources;
using Frog.Support;
using Frog.System.Specs.Underware;
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
                                           new CompoundTaskSource(
                                                                  new NUnitTaskDetctor(fileFinder),
                                                                  new MSBuildDetector(fileFinder)),
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
        public void sshould_execute_nunit_task()
        {
            execTaskFactory.Received().CreateTask("nunit", Arg.Any<string>(),
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