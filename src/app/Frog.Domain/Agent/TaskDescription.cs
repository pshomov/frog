using System.Collections.Generic;

namespace Frog.Domain
{
    public abstract class TaskDescription
    {
        public string Name;

        public List<ExecutableTask> GimeTasks(ExecTaskGenerator gen)
        {
            return gen.GimeTasks((dynamic)this);
        }
    }

    public class TestTaskDescription : TaskDescription
    {
        public readonly string Path;

        public TestTaskDescription(string path)
        {
            Path = path;
        }

    }

    public class FakeTaskDescription : TaskDescription
    {
        public readonly string[] messages;

        public FakeTaskDescription(params string[] messages)
        {
            this.messages = messages;
        }
    }

    public class ShellTaskDescription : TaskDescription
    {
        public string Command;
        public string Arguments;
    }
}