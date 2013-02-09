using System.Collections.Generic;
using System.Runtime.Serialization;
using Lokad.Cqrs;
using Lokad.Cqrs.AtomicStorage;
using SaaS.Engine;

namespace SaaS.Client.Projections.Releases
{
    [DataContract]
    public sealed class Projects
    {
        [DataMember(Order = 1)]
        public List<Project> Items { get; set; }

        public Projects()
        {
            Items = new List<Project>();
        }
    }

    [DataContract]
    public sealed class Project
    {
        [DataMember(Order = 1)]
        public string Url { get; set; }
    }


    public class ProjectsListViewProjection
    {
        readonly IDocumentWriter<unit, Projects> writer;

        public ProjectsListViewProjection(IDocumentWriter<unit, Projects> writer)
        {
            this.writer = writer;
        }

        public void When(ProjectRegistered e)
        {
            writer.UpdateEnforcingNew(unit.it, view => view.Items.Add(new Project
                {
                    Url = e.RepoUrl
                }));
        }
    }
}