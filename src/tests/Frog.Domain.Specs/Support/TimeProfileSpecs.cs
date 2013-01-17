using System;
using System.Threading;
using Frog.Support;
using NSubstitute;
using NUnit.Framework;

namespace Frog.Domain.Specs.Support
{
    [TestFixture]
    class TimeProfileSpecs
    {
        [Test]
        public void should_report_start_and_end_time_of_measure()
        {
            Profiler.MeasurementsBridge = Substitute.For<Profiler.LoggingBridge>();
            var now = DateTime.Now;
            using(Profiler.measure("ff"))
            {
                Thread.Sleep(200);
            }
            Profiler.MeasurementsBridge.Received().TimePeriod(Arg.Is<DateTime>(time => (time - now).Ticks  <= 1000000), Arg.Is<DateTime>(time => (time - now).Ticks <= 3000000), "ff");
        }
    }
}
