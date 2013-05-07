using System;
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
            agent.Start(Guid.Parse("38892791-8E48-4B95-90C3-B16CFA9BEFF8"));
        }

        protected override void OnStop()
        {
            agent.Dispose();
        }
    }
}
