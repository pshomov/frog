using System.IO;
using Frog.Domain;
using Frog.Domain.CustomTasks;
using Frog.Domain.Specs;
using Frog.Domain.TaskSources;
using Frog.Specs.Support;
using Frog.Support;
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
            valve.RegisterNewProject(new GitDriver("", "name", repo.Url));
            var changeset = GetChangesetArea();
            var genesis = new FileGenesis(changeset);
            genesis
                .File("SampleProject.sln", "")
                .Folder("src")
                    .Folder("tests")
                        .Folder("Some.Tests")
                            .File("Some.Test.csproj", "");
            repo.CommitDirectoryTree(changeset);
            valve.Check();
        }

        [Test]
        public void should_execute_xbuild_task()
        {
            execTaskFactory.Received().CreateTask("xbuild", Os.DirChars("SampleProject.sln"), Arg.Any<string>());
        }

        string GetChangesetArea()
        {
            var changeset = Path.Combine(GitTestSupport.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(changeset);
            return changeset;
        }
    }
}