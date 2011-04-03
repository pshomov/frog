using System;
using System.Collections.Generic;
using System.Threading;

namespace SimpleCQRS
{
    public interface IBus : ICommandSender, IEventPublisher
    {
        void RegisterHandler<T>(Action<T> handler) where T : Message;
    }

    public interface IBusDebug
    {
        event Action<Message> OnMessage;
    }

    public class FakeBus : IBus, IBusDebug
    {
        private readonly Dictionary<Type, List<Action<Message>>> _routes = new Dictionary<Type, List<Action<Message>>>();

        public void RegisterHandler<T>(Action<T> handler) where T : Message
        {
            List<Action<Message>> handlers;
            if(!_routes.TryGetValue(typeof(T), out handlers))
            {
                handlers = new List<Action<Message>>();
                _routes.Add(typeof(T), handlers);
            }
            handlers.Add(DelegateAdjuster.CastArgument<Message, T>(x => handler(x)));
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
            else
            {
                throw new InvalidOperationException("no handler registered");
            }
        }

        public void Publish<T>(T @event) where T : Event
        {
            OnMessage(@event);
            List<Action<Message>> handlers; 
            if (!_routes.TryGetValue(@event.GetType(), out handlers)) return;
            foreach(var handler in handlers)
            {
                //dispatch on thread pool for added awesomeness
                var handler1 = handler;
//                ThreadPool.QueueUserWorkItem(x => handler1(@event));
                handler1(@event);
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
        void Publish<T>(T @event) where T : Event;
    }
}
