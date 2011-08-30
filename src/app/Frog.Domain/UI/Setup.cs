using System.Collections.Concurrent;
using SimpleCQRS;

namespace Frog.Domain.UI
{
    public class Setup
    {
        public static ConcurrentDictionary<string,BuildStatus> SetupView(IBus theBus)
        {
            var report = new ConcurrentDictionary<string, BuildStatus>();
            var statusView = new PipelineStatusView(report);
            theBus.RegisterHandler<BuildStarted>(statusView.Handle, "UI");
            theBus.RegisterHandler<BuildEnded>(statusView.Handle, "UI");
            theBus.RegisterHandler<BuildUpdated>(statusView.Handle, "UI");
            theBus.RegisterHandler<TerminalUpdate>(statusView.Handle, "UI");
            return report;
        }
    }
}
