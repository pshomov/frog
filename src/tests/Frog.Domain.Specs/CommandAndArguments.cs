using Frog.Domain.BuildSystems;
using Frog.Specs.Support;
using NSubstitute;
using NUnit.Framework;

namespace Frog.Domain.Specs
{
    [TestFixture]
    public class CommandAndArguments : BDD
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
            execTaskGenerator.GimeTasks(new ShellTask(){cmd = "ccc", args = "/a /b"});
        }

        [Test]
        public void should_have_bundle_task()
        {
            execTaskFactory.Received().CreateTask(Arg.Is<string>(s => s == "cmd.exe" || s == "/bin/bash"), Arg.Is<string>(s => s == "-c \"ccc /a /b\"" || s == "/c ccc /a /b"), "Shell Task");
        }
    }

    [TestFixture]
    public class NoCommandJustArguments : BDD
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
            execTaskGenerator.GimeTasks(new ShellTask(){cmd = "", args = "ccc /a /b"});
        }

        [Test]
        public void should_generate_shell_command()
        {
            execTaskFactory.Received().CreateTask(Arg.Is<string>(s => s == "cmd.exe" || s == "/bin/bash"), Arg.Is<string>(s => s == "-c \"ccc /a /b\"" || s == "/c ccc /a /b"), "Shell Task");
        }
    }
}