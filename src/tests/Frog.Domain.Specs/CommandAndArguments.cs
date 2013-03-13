using Frog.Domain.Integration;
using Frog.Specs.Support;
using NSubstitute;
using NUnit.Framework;

namespace Frog.Domain.Specs
{
    [TestFixture]
    public class CommandAndArguments : BDD
    {
        Integration.ExecutableTaskGenerator executableTaskGenerator;
        ExecTaskFactory execTaskFactory;

        protected override void Given()
        {
            execTaskFactory = Substitute.For<ExecTaskFactory>();
            executableTaskGenerator = new Integration.ExecutableTaskGenerator(execTaskFactory);
        }

        protected override void When()
        {
            executableTaskGenerator.GimeTasks(new ShellTaskDescription(){Command = "ccc", Arguments = "/a /b", Name = "Shell Task"});
        }

        [Test]
        public void should_have_bundle_task()
        {
            execTaskFactory.Received().CreateOSExecutableTask(Arg.Is<string>(s => s == "cmd.exe" || s == "/bin/bash"), Arg.Is<string>(s => s == "-c \"ccc /a /b\"" || s == "/c ccc /a /b"), "Shell Task");
        }
    }

    [TestFixture]
    public class NoCommandJustArguments : BDD
    {
        Integration.ExecutableTaskGenerator executableTaskGenerator;
        ExecTaskFactory execTaskFactory;

        protected override void Given()
        {
            execTaskFactory = Substitute.For<ExecTaskFactory>();
            executableTaskGenerator = new Integration.ExecutableTaskGenerator(execTaskFactory);
        }

        protected override void When()
        {
            executableTaskGenerator.GimeTasks(new ShellTaskDescription(){Command = "", Arguments = "ccc /a /b"});
        }

        [Test]
        public void should_generate_shell_command()
        {
            execTaskFactory.Received().CreateOSExecutableTask(Arg.Is<string>(s => s == "cmd.exe" || s == "/bin/bash"), Arg.Is<string>(s => s == "-c \"ccc /a /b\"" || s == "/c ccc /a /b"), Arg.Any<string>());
        }
    }
    [TestFixture]
    public class TaskNameDefaultsToTheCommand : BDD
    {
        Integration.ExecutableTaskGenerator executableTaskGenerator;
        ExecTaskFactory execTaskFactory;

        protected override void Given()
        {
            execTaskFactory = Substitute.For<ExecTaskFactory>();
            executableTaskGenerator = new Integration.ExecutableTaskGenerator(execTaskFactory);
        }

        protected override void When()
        {
            executableTaskGenerator.GimeTasks(new ShellTaskDescription(){Command = "", Arguments = "ccc /a /b"});
        }

        [Test]
        public void should_generate_shell_command()
        {
            execTaskFactory.Received().CreateOSExecutableTask(Arg.Is<string>(s => s == "cmd.exe" || s == "/bin/bash"), Arg.Is<string>(s => s == "-c \"ccc /a /b\"" || s == "/c ccc /a /b"), Arg.Is<string>(s => s == "cmd.exe /c ccc /a /b" || s == "/bin/bash -c \"ccc /a /b\""));
        }
    }
}