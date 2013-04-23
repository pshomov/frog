using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace SimpleCQRS
{
    public interface IBus : ICommandSender, IEventPublisher
    {
        void UnRegisterHandler<T>(Action<T> handler, string handlerId) where T : Message;
        void RegisterHandler<T>(Action<T> handler, string handlerId) where T : Message;
        void RegisterAll(Action<string, string> handler);
        void UnRegisterAll();
    }

    public interface IBusDebug
    {
        event Action<Message> OnMessage;
    }

    public class FakeBus : IBus, IBusDebug
    {
        private readonly Dictionary<Type, Dictionary<string, Action<Message>>> _routes = new Dictionary<Type, Dictionary<string, Action<Message>>>();
        Action<string, string> _all_handler = (s, s1) => {};

        public void UnRegisterHandler<T>(Action<T> handler, string handlerId) where T : Message
        {
            _routes[typeof (T)].Remove(handlerId);
        }

        public void RegisterHandler<T>(Action<T> handler, string handlerId) where T : Message
        {
            Dictionary<string, Action<Message>> handlers;
            if(!_routes.TryGetValue(typeof(T), out handlers))
            {
                handlers = new Dictionary<string, Action<Message>>();
                _routes.Add(typeof(T), handlers);
            }
            handlers.Add(handlerId, DelegateAdjuster.CastArgument<Message, T>(x => handler(x)));
        }

        public void RegisterAll(Action<string, string> handler)
        {
            _all_handler = handler;
        }

        public void UnRegisterAll()
        {
            _all_handler = (s, s1) => { };
        }

        public void Send<T>(T command) where T : Command
        {
            OnMessage(command);
            Dictionary<string, Action<Message>> handlers; 
            if (_routes.TryGetValue(typeof(T), out handlers))
            {
                if (handlers.Count != 1) throw new InvalidOperationException("cannot send to more than one handler");
                handlers.Values.First()(command);
            }
            var serialized = JsonConvert.SerializeObject(command);
            _all_handler(serialized, typeof(T).Name);
        }

        public void Send<T>(T command, string handlerId) where T : Command
        {
            OnMessage(command);
            Dictionary<string, Action<Message>> handlers;
            if (_routes.TryGetValue(typeof(T), out handlers))
            {
                handlers[handlerId](command);
            }
            var serialized = JsonConvert.SerializeObject(command);
            _all_handler(serialized, typeof(T).Name);
        }

        public void Publish<T>(T @event) where T : Event
        {
            OnMessage(@event);
            Dictionary<string, Action<Message>> handlers; 
            if (_routes.TryGetValue(@event.GetType(), out handlers))
            {
                foreach(var handler in handlers.Values)
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
        void Send<T>(T command, string handlerId) where T : Command;
    }
    public interface IEventPublisher
    {
        void Publish<T>(T @event) where T : Event;
    }
}
