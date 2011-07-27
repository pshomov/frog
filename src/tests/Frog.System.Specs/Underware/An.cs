using System;
using System.Collections.Generic;
using System.Linq;
using NHamcrest;
using SimpleCQRS;

namespace Frog.System.Specs.Underware
{
    public class InlineMatcher<T> : Matcher<T>
    {
        readonly NHamcrest.Func<T, bool> assertion;

        public InlineMatcher(NHamcrest.Func<T, bool> assertion)
        {
            this.assertion = assertion;
        }

        public override bool Matches(T item)
        {
            return assertion(item);
        }
    }


    public class EventMatcher<T> : Matcher<List<Message>> where T : Event
    {
        readonly NHamcrest.Func<T, bool>[] matchers;

        public EventMatcher(params NHamcrest.Func<T, bool>[] matchers)
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
        readonly NHamcrest.Func<T, bool>[] matchers;

        public CommandMatcher(params NHamcrest.Func<T, bool>[] matchers)
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
        public static IMatcher<List<Message>> Event<T>(params NHamcrest.Func<T, bool>[] matchers) where T : Event
        {
            return new EventMatcher<T>(matchers);
        }
    }

    public static class A
    {
        public static IMatcher<T> Check<T>(NHamcrest.Func<T, bool> matchers)
        {
            return new InlineMatcher<T>(matchers);
        }

        public static IMatcher<List<Message>> Command<T>(params NHamcrest.Func<T, bool>[] matchers) where T : Command
        {
            return new CommandMatcher<T>(matchers);
        }
    }
}