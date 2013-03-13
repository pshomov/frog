using System;

namespace Frog.Domain
{
    public abstract class BuildEvent : OrderedEvent
    {
        public Guid BuildId { get; set; }

        protected BuildEvent() : this(Guid.Empty, -1)
        {
        }

        protected BuildEvent(Guid buildId, int sequenceId) : base(sequenceId)
        {
            BuildId = buildId;
        }
    }
}