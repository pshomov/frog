using System.Collections.Concurrent;
using Frog.Domain.Integration.UI;
using SimpleCQRS;

namespace Frog.UI.Web
{
    public static class ServiceLocator
    {
        public static TerminalOutputView TerminalOutputStatus { get; set; }
        public static ProjectView ProjectStatus { get; set; }
        public static BuildView BuildStatus { get; set; }
        public static IBus Bus { get; set; }
        public static ConcurrentQueue<Message> AllMessages { get; set; }
    }
}