using SimpleCQRS;

namespace Frog.Domain.Underware
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