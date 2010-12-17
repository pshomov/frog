using System;
using System.Collections.Generic;
using Machine.Specifications;
using NHamcrest;
using NHamcrest.Core;
using xray;

namespace Frog.System.Specs
{
    internal class NestedMatcher<T, V> : IMatcher<T>
    {
        private readonly IMatcher<V> _matcher;
        private readonly NHamcrest.Func<T, V> _query;

        public NestedMatcher(NHamcrest.Func<T, V> query, IMatcher<V> matcher)
        {
            _query = query;
            _matcher = matcher;
        }

        #region IMatcher<T> Members

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

        #endregion
    }

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
            Prober.check(Snapshot(() => _theDriver.Stream1).
                             Has(x => x.GotWater, Is.EqualTo(true)).
                             Has(x => x, Is.NotNull())
                );
        }

        private static SnapshotExpectationsBuilder<T> Snapshot<T>(Func<T> snapshotTaker)
        {
            return new ValueMatchingProbe2<T>(snapshotTaker);
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

    public interface SnapshotExpectationsBuilder<T> : Probe
    {
        void AddMatcher<V>(NHamcrest.Func<T, V> query, IMatcher<V> matcher);
    }

    public static class SnapshotHelpers
    {
        public static SnapshotExpectationsBuilder<T> Has<T, V>(this SnapshotExpectationsBuilder<T> f,
                                                               NHamcrest.Func<T, V> query, IMatcher<V> matcher)
        {
            f.AddMatcher(query, matcher);
            return f;
        }
    }

    public class ValueMatchingProbe2<T> : SnapshotExpectationsBuilder<T>
    {
        private readonly List<IMatcher<T>> _matcher = new List<IMatcher<T>>();
        private readonly Func<T> _snapshotTaker;

        public ValueMatchingProbe2(Func<T> snapshotTaker)
        {
            _snapshotTaker = snapshotTaker;
        }

        private IMatcher<T> Matcher
        {
            get { return Matches.AllOf(_matcher); }
        }

        public void AddMatcher<V>(NHamcrest.Func<T, V> query, IMatcher<V> matcher)
        {
            _matcher.Add(new NestedMatcher<T, V>(query, matcher));
        }

        public bool probeAndMatch()
        {
            return Matcher.Matches(_snapshotTaker());
        }
    }
}