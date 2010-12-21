namespace Frog.Domain
{
    public interface Task
    {
        void Perform(SourceDrop sourceDrop);
    }
}