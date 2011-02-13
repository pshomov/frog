using System.Collections.Generic;
using System.Linq;
using NHamcrest;
using SimpleCQRS;

namespace Frog.System.Specs
{
    public class EventMatcher<T> : Matcher<List<Event>> where T : Event
    {
        readonly Func<T, bool>[] matchers;

        public EventMatcher(params Func<T, bool>[] matchers)
        {
            this.matchers = matchers;
        }

        public override bool Matches(List<Event> events)
        {
            return
                events.Where(@event => @event.GetType() == typeof (T)).Any(
                    @event => matchers.All(func => func(@event as T)));
        }
    }

    public static class An
    {
        public static IMatcher<List<Event>> Event<T>(params Func<T, bool>[] matchers) where T : Event
        {
            return new EventMatcher<T>(matchers);
        }
    }
}