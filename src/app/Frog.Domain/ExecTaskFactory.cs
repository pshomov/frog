namespace Frog.Domain
{
    public interface ExecTaskFactory
    {
        ExecTask CreateTask(string app, string args);
    }
}