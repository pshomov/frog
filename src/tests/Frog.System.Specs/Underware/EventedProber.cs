﻿using NHamcrest.Core;
using SimpleCQRS;
using xray;

namespace Frog.System.Specs.Underware
{
    public class EventedProber
    {
        readonly int timeout;
        readonly FakeBus bus;

        public EventedProber(int timeout, FakeBus bus)
        {
            this.timeout = timeout;
            this.bus = bus;
        }

        public bool check<T>() where T : Message
        {
            bool signaled = false;
            bus.RegisterHandler<T>(obj => signaled = true);
            var p = new PollingProber(timeout, 50);
            return p.check(Take.Snapshot(() => "").Has(s => signaled, Is.True()));
        }

        void dummy()
        {
            
        }
    }
}