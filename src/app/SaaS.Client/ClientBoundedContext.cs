using System.Collections.Generic;
using Lokad.Cqrs;
using Lokad.Cqrs.AtomicStorage;
using SaaS.Client.Projections.Frog.Projects;
using SaaS.Client.Projections.Releases;
using SaaS.Engine;

namespace SaaS.Client
{
    public static class ClientBoundedContext
    {
        public static IEnumerable<object> Projections(IDocumentStore docs)
        {
//            yield return new ProjectsListViewProjection(docs.GetWriter<unit, Projects>());
            yield return new ProjectHistoryProjectionView(docs.GetWriter<ProjectId, ProjectHistory>());
            yield return new BuildViewProjection(docs.GetWriter<BuildId, Build>());
        }
    }
}