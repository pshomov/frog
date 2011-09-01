using Frog.Domain.CustomTasks;

namespace Frog.Domain.BuildSystems.Rake
{
    class AnyTask : ITask
    {
        public string cmd;
        public string args;
    }
}