using System;
using System.Collections.Generic;
using System.Threading;

namespace SimpleCQRS
{
    public interface IBus : ICommandSender, IEventPublisher
    {
        void RegisterHandler<T>(Action<T> handler, string handlerId) where T : Message;
        void RegisterHandler(Action<string, string> handler, string handlerId);
    }

    public interface IBusDebug
    {
        event Action<Message> OnMessage;
    }

    public class FakeBus : IBus, IBusDebug
    {
        private readonly Dictionary<Type, List<Action<Message>>> _routes = new Dictionary<Type, List<Action<Message>>>();

        public void RegisterHandler<T>(Action<T> handler, string handlerId) where T : Message
        {
            List<Action<Message>> handlers;
            if(!_routes.TryGetValue(typeof(T), out handlers))
            {
                handlers = new List<Action<Message>>();
                _routes.Add(typeof(T), handlers);
            }
            handlers.Add(DelegateAdjuster.CastArgument<Message, T>(x => handler(x)));
        }

        public void RegisterHandler(Action<string, string> handler, string handlerId)
        {
            throw new NotImplementedException();
        }

        public void Send<T>(T command) where T : Command
        {
            OnMessage(command);
            List<Action<Message>> handlers; 
            if (_routes.TryGetValue(typeof(T), out handlers))
            {
                if (handlers.Count != 1) throw new InvalidOperationException("cannot send to more than one handler");
                handlers[0](command);
            }
        }

        public void PublishOne<T>(T @event) where T : Event
        {
            OnMessage(@event);
            List<Action<Message>> handlers; 
            if (!_routes.TryGetValue(@event.GetType(), out handlers)) return;
            foreach(var handler in handlers)
            {
				handler(@event);
            }
        }

        public void Publish<T>(params T[] @event) where T : Event
        {
            foreach (var ev in @event)
            {
                PublishOne(ev);
            }
        }

        public event Action<Message> OnMessage = message => {};
    }

    public interface Handles<T>
    {
        void Handle(T message);
    }

    public interface ICommandSender
    {
        void Send<T>(T command) where T : Command;

    }
    public interface IEventPublisher
    {
        void Publish<T>(params T[] @event) where T : Event;
    }
}
