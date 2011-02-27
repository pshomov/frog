using Frog.Domain;
using Frog.Domain.CustomTasks;
using Frog.Domain.Specs;
using Frog.Domain.TaskSources;
using Frog.System.Specs.Underware;
using NSubstitute;
using NUnit.Framework;

namespace Frog.System.Specs
{
    [Ignore]
    [TestFixture]
    public class ProjectRegistration : BDD
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
            system.RegisterNewProject(repo.Url);
        }

        [Test]
        public void should_include_project_in_next_time_all_projects_need_to_be_updated()
        {
            Assert.That(false);
        }

    }
}