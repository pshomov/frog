using System;
using System.Collections.Concurrent;
using SimpleCQRS;

namespace Frog.Domain.UI
{
    public class Setup
    {
        public static Tuple<ConcurrentDictionary<Guid,BuildStatus>, ConcurrentDictionary<string, Guid>>  SetupView(IBus theBus)
        {
            var report = new ConcurrentDictionary<Guid, BuildStatus>();
            var currentBuilds = new ConcurrentDictionary<string, Guid>();
            var statusView = new PipelineStatusView(report, currentBuilds);
            theBus.RegisterHandler<BuildStarted>(statusView.Handle, "UI");
            theBus.RegisterHandler<BuildEnded>(statusView.Handle, "UI");
            theBus.RegisterHandler<BuildUpdated>(statusView.Handle, "UI");
            theBus.RegisterHandler<TerminalUpdate>(statusView.Handle, "UI");
            return new Tuple<ConcurrentDictionary<Guid, BuildStatus>, ConcurrentDictionary<string, Guid>>(report, currentBuilds);
        }
    }
}
