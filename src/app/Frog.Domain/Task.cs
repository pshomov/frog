namespace Frog.Domain
{
    public class Task
    {
    }

    public class TestTask : Task
    {
        public readonly string path;

        public TestTask(string path)
        {
            this.path = path;
        }
    }

    public class FakeTaskDescription : Task
    {
        public readonly string[] messages;

        public FakeTaskDescription(params string[] messages)
        {
            this.messages = messages;
        }
    }

    public class ShellTaskk  : Task
    {
        public string Name;
        public string Command;
        
    }
}