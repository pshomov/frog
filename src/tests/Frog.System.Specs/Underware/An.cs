﻿using System;
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


    public class MessageMatcher<T> : Matcher<List<Message>> where T : class
    {
        private readonly int occurences;
        private NHamcrest.Func<T, bool>[] matchers;

        public MessageMatcher(int occurences, params NHamcrest.Func<T, bool>[] matchers)
        {
            this.occurences = occurences;
            this.matchers = matchers;
        }

        public override bool Matches(List<Message> messages)
        {
            var matchedMessages = messages.Where(message => message.GetType() == typeof (T)).Where(
                message => matchers.All(func => func(message as T)));
            return matchedMessages.Count() == occurences;
        }
    }

    public class EventMatcher<T> : MessageMatcher<T> where T : Event
    {
        public EventMatcher(int occurances = 1, params NHamcrest.Func<T, bool>[] matchers) : base(occurances, matchers)
        {
        }
    }

    public class CommandMatcher<T> : MessageMatcher<T> where T : Command
    {
        public CommandMatcher(int occurances, params NHamcrest.Func<T, bool>[] matchers) : base(occurances, matchers)
        {
        }
    }

    public static class An
    {
        public static IMatcher<List<Message>> Event<T>(params NHamcrest.Func<T, bool>[] matchers) where T : Event
        {
            return new EventMatcher<T>(1, matchers);
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
            return new CommandMatcher<T>(1, matchers);
        }
    }
    public static class Two
    {
        public static IMatcher<List<Message>> Commands<T>(params NHamcrest.Func<T, bool>[] matchers) where T : Command
        {
            return new CommandMatcher<T>(2, matchers);
        }
    }
}