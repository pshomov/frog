using System;
using NHamcrest;
using NHamcrest.Core;
using xray;
using System.Collections.Generic;


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
	
	public class Take {
        public static SnapshotExpectationsBuilder<T> Snapshot<T>(Func<T> snapshotTaker)
        {
            return new ValueMatchingProbe2<T>(snapshotTaker);
        }
	}
}

