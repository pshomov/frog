namespace Frog.Domain
{
    public enum TaskResult
    {
        Success,
        Error
    }
    public interface Task
    {
        TaskResult Perform(SourceDrop sourceDrop);
    }
}