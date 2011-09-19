using System;
using System.Collections.Concurrent;
using Frog.Domain;
using Frog.Domain.UI;
using SimpleCQRS;

namespace Frog.UI.Web
{
    public static class ServiceLocator
    {
        public static ProjectView Report { get; set; }
        public static ConcurrentDictionary<string, Guid> CurrentReport { get; set; }
        public static IBus Bus { get; set; }
        public static ConcurrentQueue<Message> AllMassages { get; set; }
    }
}