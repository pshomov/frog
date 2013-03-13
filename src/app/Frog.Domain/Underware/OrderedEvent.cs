using SimpleCQRS;

namespace Frog.Domain
{
    public abstract class OrderedEvent : Event
    {
        public int SequenceId;

        protected OrderedEvent(int sequenceId)
        {
            SequenceId = sequenceId;
        }
    }
}