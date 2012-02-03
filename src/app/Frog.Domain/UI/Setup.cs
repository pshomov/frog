using EventStore;
using SimpleCQRS;

namespace Frog.Domain.UI
{
    public class Setup
    {
        public static void SetupView(IBus theBus, IStoreEvents eventStore)
        {
            var statusView = new PipelineStatusView(eventStore);
            theBus.RegisterHandler<BuildStarted>(statusView.Handle, "UI");
            theBus.RegisterHandler<BuildEnded>(statusView.Handle, "UI");
            theBus.RegisterHandler<BuildUpdated>(statusView.Handle, "UI");
            theBus.RegisterHandler<TerminalUpdate>(statusView.Handle, "UI");
            theBus.RegisterHandler<ProjectCheckedOut>(statusView.Handle, "UI");
        }
    }
}