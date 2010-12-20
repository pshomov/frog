using System;
using System.Collections.Generic;
using Machine.Specifications;
using NHamcrest;
using NHamcrest.Core;
using xray;

namespace Frog.System.Specs
{


    public class Attempting_a_system_test_on_my_own
    {
        private static TestDriver _theDriver;

        private static PollingProber Prober = new PollingProber(10000, 100);

        private Establish context = () => { _theDriver = new TestDriver(); };

        private Because of = () => _theDriver.SourceFontain.HasSplash();

        private It should_do_one_more = () => Test();

        public static void Test()
        {
            Prober.check(Take.Snapshot(() => _theDriver.Stream1).
                             Has(x => x.GotWater, Is.EqualTo(true)).
                             Has(x => x, Is.NotNull())
                );
        }


    }

}