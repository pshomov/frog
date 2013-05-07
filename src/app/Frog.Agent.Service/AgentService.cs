using System.ServiceProcess;
using Frog.WiredUp;

namespace Frog.Agent.Service
{
    public partial class AgentService : ServiceBase
    {
        AgentDeploumentWireUp agent;

        public AgentService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            agent = new AgentDeploumentWireUp();
        }

        protected override void OnStop()
        {
            agent.Dispose();
        }
    }
}
