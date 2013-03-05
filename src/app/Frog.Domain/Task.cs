using System.Collections.Generic;

namespace Frog.Domain
{
    public abstract class Task
    {
        public abstract List<IExecTask> GimeTasks(IExecTaskGenerator gen);
    }

    public class TestTask : Task
    {
        public readonly string path;

        public TestTask(string path)
        {
            this.path = path;
        }

        public override List<IExecTask> GimeTasks(IExecTaskGenerator gen)
        {
                return gen.GimeTasks(this);
        }
    }

    public class FakeTaskDescription : Task
    {
        public readonly string[] messages;

        public FakeTaskDescription(params string[] messages)
        {
            this.messages = messages;
        }
        public override List<IExecTask> GimeTasks(IExecTaskGenerator gen)
        {
            return gen.GimeTasks(this);
        }
    }

    public class ShellTaskk  : Task
    {
        public string Name;
        public string Command;
        public string Arguments;
        public override List<IExecTask> GimeTasks(IExecTaskGenerator gen)
        {
            return gen.GimeTasks(this);
        }
    }
}