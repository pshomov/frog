namespace Frog.Domain
{
    public class ExecTaskFactory
    {
        public virtual ExecTask CreateTask(string app, string args, string name)
        {
            return new ExecTask(name, app, args);
        }
    }
}