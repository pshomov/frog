namespace Frog.Domain
{
    public class ExecTaskFactory
    {
        public virtual ExecTask CreateTask(string app, string args)
        {
            return new ExecTask(app, args);
        }
    }
}