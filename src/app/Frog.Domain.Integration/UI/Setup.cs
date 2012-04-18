using EventStore;
using Frog.Support;
using SimpleCQRS;

namespace Frog.Domain.Integration.UI
{
    public static class Setup
    {
        public static void SetupView(IBus theBus, IStoreEvents eventStore)
        {
            RegisterProjectStatusHandler(theBus, eventStore);
            RegisterTerminalUpdateHandler(theBus, eventStore);
        }

        static void RegisterProjectStatusHandler(IBus theBus, IStoreEvents eventStore)
        {
            var statusView = new PipelineStatusEventHandler(eventStore);
            theBus.RegisterHandler<BuildStarted>(statusView.Handle, "UI");
            theBus.RegisterHandler<BuildEnded>(statusView.Handle, "UI");
            theBus.RegisterHandler<BuildUpdated>(statusView.Handle, "UI");
            theBus.RegisterHandler<ProjectCheckedOut>(statusView.Handle, "UI");
        }

        static void RegisterTerminalUpdateHandler(IBus theBus, IStoreEvents eventStore)
        {
            var eventView = new TerminalOutputEventHandler(new TerminalOutputRegister(OSHelpers.TerminalViewConnection()));
            theBus.RegisterHandler<TerminalUpdate>(eventView.Handle, "UI.Terminal");
        }
    }
}