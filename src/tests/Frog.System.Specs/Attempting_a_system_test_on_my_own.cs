using System;
using Machine.Specifications;
using NHamcrest;
using NHamcrest.Core;
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
    public class SpecsHelper<T, V> 
    {
        public static IMatcher<T> Has(NHamcrest.Func<T,V> checker,  IMatcher<V> matcher)
        {
            return new NestedMatcher<T,V>(checker, matcher);
        }
    }
    internal class ValueMatchingProbe : Probe
    {
        readonly IMatcher<bool> matcher;

        public ValueMatchingProbe(IMatcher<bool> matcher)
        {
            this.matcher = matcher;
        }

        public bool probeAndMatch()
        {
            return matcher.Matches(true);
        }
    }
    public class Attempting_a_system_test_on_my_own
    {
        static TestDriver _theDriver;

        Establish context = () =>
                                {
                                    _theDriver = new TestDriver();
                                };

        Because of = () => _theDriver.SourceFontain.HasSplash();

        It should_start_the_pipeline =
            () => new PollingProber(5000, 100).check(new ValueMatchingProbe(SpecsHelper<bool,bool>.Has(arg => _theDriver.Stream1.GotWater, Is.True())));
        public void Test()
        {
            
        }
    }

}