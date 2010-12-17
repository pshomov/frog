using System;
using System.Collections.Generic;
using Machine.Specifications;
using NHamcrest;
using NHamcrest.Core;
using xray;

namespace Frog.System.Specs
{

    internal class ValueMatchingProbe<T> : Probe
    {
        private readonly IMatcher<T> matcher;
        private readonly Func<T> Snapshot;

        public ValueMatchingProbe(Func<T> snapshot, IMatcher<T> matcher)
        {
            Snapshot = snapshot;
            this.matcher = matcher;
        }

        #region Probe Members

        public bool probeAndMatch()
        {
            return matcher.Matches(Snapshot());
        }

        #endregion
    }

    public class Spec
    {
        public static IMatcher<T> Has<T, V>(NHamcrest.Func<T, V> checker, IMatcher<V> matcher)
        {
            return new NestedMatcher<T, V>(checker, matcher);
        }
    }

    public class Attempting_a_system_test_on_my_own
    {
        private static TestDriver _theDriver;

        private static PollingProber Prober = new PollingProber(10000, 100);

        private Establish context = () => { _theDriver = new TestDriver(); };

        private Because of = () => _theDriver.SourceFontain.HasSplash();

        private It should_start_the_pipeline =
            () =>
            Prober.check(ValueProbe(() => _theDriver.Stream1, has<FakeStream, bool>(x => x.GotWater, Is.EqualTo(true))));

        private It should_do_one_more = () => Test();

        public static void Test()
        {
            Prober.check(Take.Snapshot(() => _theDriver.Stream1).
                             Has(x => x.GotWater, Is.EqualTo(true)).
                             Has(x => x, Is.NotNull())
                );
        }


        private static Probe ValueProbe<T>(Func<T> query, IMatcher<T> matcher)
        {
            return new ValueMatchingProbe<T>(query, matcher);
        }

        private static IMatcher<T> has<T, V>(NHamcrest.Func<T, V> query, IMatcher<V> matcher)
        {
            return Spec.Has(query, matcher);
        }
    }

}