using System;
using System.Collections.Generic;
using System.Threading;
using Newtonsoft.Json;

namespace SimpleCQRS
{
    public interface IBus : ICommandSender, IEventPublisher
    {
        void RegisterHandler<T>(Action<T> handler, string handlerId) where T : Message;
        void RegisterAll(Action<string, string> handler);
    }

    public interface IBusDebug
    {
        event Action<Message> OnMessage;
    }

    public class FakeBus : IBus, IBusDebug
    {
        private readonly Dictionary<Type, List<Action<Message>>> _routes = new Dictionary<Type, List<Action<Message>>>();
        Action<string, string> _all_handler = (s, s1) => {};

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

        public void RegisterAll(Action<string, string> handler)
        {
            _all_handler = handler;
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
            var serialized = JsonConvert.SerializeObject(command);
            _all_handler(serialized, typeof(T).Name);
        }

        public void Publish<T>(T @event) where T : Event
        {
            OnMessage(@event);
            List<Action<Message>> handlers; 
            if (_routes.TryGetValue(@event.GetType(), out handlers))
            {
                foreach(var handler in handlers)
                {
                    var handler1 = handler;
//                    ThreadPool.QueueUserWorkItem(state => handler1(@event));
                    handler1(@event);
                }
            }
            var serialized = JsonConvert.SerializeObject(@event);
//            ThreadPool.QueueUserWorkItem(state => _all_handler(serialized, typeof (T).Name));
            _all_handler(serialized, typeof (T).Name);
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
