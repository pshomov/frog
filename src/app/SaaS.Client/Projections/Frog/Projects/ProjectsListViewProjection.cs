using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Lokad.Cqrs;
using Lokad.Cqrs.AtomicStorage;
using SaaS.Engine;

namespace SaaS.Client.Projections.Frog.Projects
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

        public bool IsProjectRegistered(string projectUrl)
        {
            return Items.Count(project => project.Url == projectUrl) == 1;
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