namespace Frog.Domain
{
    public class TaskResult
    {
        public enum Status
        {
            Success,
            Error
        };

        public Status status;
    }

    public interface Task
    {
        TaskResult Perform(SourceDrop sourceDrop);
    }
}