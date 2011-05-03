using System;
using System.Collections.Concurrent;
using Frog.Domain;
using Frog.Domain.UI;
using SimpleCQRS;

namespace Frog.UI.Web
{
    public static class ServiceLocator
    {
        public static ConcurrentDictionary<string, BuildStatus> Report { get; set; }
        public static IBus Bus { get; set; }
        public static ConcurrentQueue<Message> AllMassages { get; set; }
    }
}