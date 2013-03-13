using System.ServiceProcess;
using Frog.Domain;
using Frog.Domain.Integration;

namespace Frog.Agent.Service
{
    public partial class AgentService : ServiceBase
    {
        public AgentService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            var worker = new Worker(Frog.Agent.Program.GetPipeline(), Frog.Agent.Program.SetupWorkingAreaGovernor());
            var bus = Frog.Agent.Program.SetupBus();
            SourceRepoDriverFactory sourceRepoDriverFactory = url => new GitDriver(url);
            var agent = new Domain.Agent(bus, worker, sourceRepoDriverFactory);
            var revisionChecker = new RevisionChecker(bus, sourceRepoDriverFactory);

            agent.JoinTheParty();
            revisionChecker.JoinTheParty();
        }

        protected override void OnStop()
        {
        }
    }
}
