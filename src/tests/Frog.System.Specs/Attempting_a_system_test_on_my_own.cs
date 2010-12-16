using System;
using Machine.Specifications;
using NHamcrest;
using NHamcrest.Core;
using Rhino.Mocks;
using xray;

namespace Frog.System.Specs
{
    internal class NestedMatcher<T,V> : IMatcher<T>
    {
        readonly NHamcrest.Func<T, V> _query;
        readonly IMatcher<V> _matcher;

        public NestedMatcher(NHamcrest.Func<T, V> query, IMatcher<V> matcher)
        {
            _query = query;
            _matcher = matcher;
        }

        public void DescribeTo(IDescription description)
        {
            throw new NotImplementedException();
        }

        public bool Matches(T item)
        {
            return _matcher.Matches(_query(item));
        }

        public void DescribeMismatch(T item, IDescription mismatchDescription)
        {
            throw new NotImplementedException();
        }
    }

    internal class ValueMatchingProbe<T> : Probe
    {
        private readonly Func<T> Snapshot;
        readonly IMatcher<T> matcher;

        public ValueMatchingProbe(Func<T> snapshot, IMatcher<T> matcher)
        {
            Snapshot = snapshot;
            this.matcher = matcher;
        }

        public bool probeAndMatch()
        {
            return matcher.Matches(Snapshot());
        }
    }

    public class Spec
    {
        public static IMatcher<T> Has<T,V>(NHamcrest.Func<T, V> checker, IMatcher<V> matcher)
        {
            return new NestedMatcher<T, V>(checker, matcher);
        }
    }

    public class Attempting_a_system_test_on_my_own
    {
        static TestDriver _theDriver;

        private static PollingProber Prober = new PollingProber(10000, 100);

        Establish context = () =>
                                {
                                    _theDriver = new TestDriver();
                                };

        Because of = () => _theDriver.SourceFontain.HasSplash();

        It should_start_the_pipeline =
            () => Prober.check(valueProbe(() => _theDriver.Stream1, has<FakeStream, bool>(x => x.GotWater, Is.EqualTo(true))));

        static Probe valueProbe<T>(Func<T> query, IMatcher<T> matcher)
        {
            return new ValueMatchingProbe<T>(query, matcher);
        }
        static IMatcher<T> has<T,V>(NHamcrest.Func<T,V> query, IMatcher<V> matcher)
        {
            return Spec.Has(query, matcher);
        }
    }

}