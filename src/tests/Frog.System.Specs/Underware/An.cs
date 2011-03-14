using System.Collections.Generic;
using System.Linq;
using NHamcrest;
using SimpleCQRS;

namespace Frog.System.Specs.Underware
{
    public class EventMatcher<T> : Matcher<List<Message>> where T : Event
    {
        readonly Func<T, bool>[] matchers;

        public EventMatcher(params Func<T, bool>[] matchers)
        {
            this.matchers = matchers;
        }

        public override bool Matches(List<Message> messages)
        {
            return
                messages.Where(message => message.GetType() == typeof (T)).Any(
                    message => matchers.All(func => func(message as T)));
        }
    }

    public class CommandMatcher<T> : Matcher<List<Message>> where T : Command
    {
        readonly Func<T, bool>[] matchers;

        public CommandMatcher(params Func<T, bool>[] matchers)
        {
            this.matchers = matchers;
        }

        public override bool Matches(List<Message> messages)
        {
            return
                messages.Where(message => message.GetType() == typeof (T)).Any(
                    message => matchers.All(func => func(message as T)));
        }
    }

    public static class An
    {
        public static IMatcher<List<Message>> Event<T>(params Func<T, bool>[] matchers) where T : Event
        {
            return new EventMatcher<T>(matchers);
        }
        public static IMatcher<List<Message>> Command<T>(params Func<T, bool>[] matchers) where T : Command
        {
            return new CommandMatcher<T>(matchers);
        }
    }
}