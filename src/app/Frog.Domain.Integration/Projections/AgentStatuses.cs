using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Lokad.Cqrs;
using Lokad.Cqrs.AtomicStorage;
using SaaS.Client.Projections.Frog.Agents;

namespace Frog.Domain.Integration.Projections
{
    public class AgentStatuses : BuildAgentStatus
    {
        private readonly IDocumentStore store;

        public AgentStatuses(IDocumentStore store)
        {
            this.store = store;
        }

        public IEnumerable<AgentInfo> AvailableAgents
        {
            get
            {
                var agent_statuses = GetView<unit, AgentsStatus>(unit.it);
                return agent_statuses.Available.Select(info => new AgentInfo(){Id = info.Id.Id.ToString(), Capabilities = new List<string>(info.Capabilities)});
            }
        }
        TEntity GetView<TKey, TEntity>(TKey id)
        {
            var view = default(TEntity);
            var retry = 0;
            while (!store.GetReader<TKey, TEntity>().TryGet(id, out view) && retry < 10)
            {
                Thread.Sleep(1000);
                retry++;
            };
            if (view == null) throw new Exception(string.Format("Could not get entity of type {0} with id {1}", typeof(TEntity).Name, id));
            return view;
        }


    }
}
