using Frog.Domain.CustomTasks;

namespace Frog.Domain.BuildSystems.Rake
{
    public class ShellTask : ITask
    {
        public string cmd = "";
        public string args;
    }
}