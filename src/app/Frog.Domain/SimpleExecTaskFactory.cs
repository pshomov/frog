namespace Frog.Domain
{
    class SimpleExecTaskFactory : ExecTaskFactory
    {
        public ExecTask CreateTask(string app, string args)
        {
            return new ExecTask(app, args);
        }
    }
}