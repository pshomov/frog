using System;
using System.Collections.Generic;
using Frog.Specs.Support;
using Frog.System.Specs.Underware;
using SimpleCQRS;
using xray;

namespace Frog.System.Specs.ProjectBuilding
{
    public abstract class SystemBDD : BDD
    {
        protected SystemDriver system;
        protected TestSystem testSystem;

        protected override void Given()
        {
            testSystem = new TestSystem();
            system = new SystemDriver(testSystem);
        }

        protected bool EventStoreCheck(
            Func<SnapshotExpectationsBuilder<List<Message>>, SnapshotExpectationsBuilder<List<Message>>> expectations)
        {
            var prober = new PollingProber(5000, 100);
            SnapshotExpectationsBuilder<List<Message>> snapshot_expectations_builder =
                expectations(Take.Snapshot(() => system.GetEventsSnapshot()));
            return prober.check(snapshot_expectations_builder);
        }

        protected override void GivenCleanup()
        {
            system.Stop();
        }
    }
}