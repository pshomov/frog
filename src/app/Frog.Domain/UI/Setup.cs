using SimpleCQRS;

namespace Frog.Domain.UI
{
    public class Setup
    {
        public static ProjectView SetupView(IBus theBus)
        {
            var projectView = new ProjectView();
            var statusView = new PipelineStatusView(projectView);
            theBus.RegisterHandler<BuildStarted>(statusView.Handle, "UI");
            theBus.RegisterHandler<BuildEnded>(statusView.Handle, "UI");
            theBus.RegisterHandler<BuildUpdated>(statusView.Handle, "UI");
            theBus.RegisterHandler<TerminalUpdate>(statusView.Handle, "UI");
            theBus.RegisterHandler<ProjectCheckedOut>(statusView.Handle, "UI");
            return projectView;
        }
    }
}