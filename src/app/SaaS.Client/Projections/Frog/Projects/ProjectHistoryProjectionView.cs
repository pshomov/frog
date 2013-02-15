using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Lokad.Cqrs;
using Lokad.Cqrs.AtomicStorage;
using SaaS.Engine;

namespace SaaS.Client.Projections.Frog.Projects
{
    [DataContract]
    public sealed class ProjectHistory
    {
        [DataMember(Order = 1)]
        public List<ProjectBuild> Items { get; set; }
        [DataMember(Order = 2)]
        public Build Current { get; set; }
        [DataMember(Order = 3)]
        public ProjectBuild CurrentHistory { get; set; }

        public ProjectHistory()
        {
            Items = new List<ProjectBuild>();
            Current = Build.None;
            CurrentHistory = ProjectBuild.None;
        }
    }

    [DataContract]
    public sealed class ProjectBuild
    {
        public static ProjectBuild None = new ProjectBuild();

        [DataMember(Order = 1)]
        public BuildId BuildId { get; set; }
        [DataMember(Order = 2)]
        public string RevisionNr { get; set; }
        [DataMember(Order = 3)]
        public string RevisionComment { get; set; }
        [DataMember(Order = 4)]
        public BuildStatus Status  { get; set; }
    }

    public enum BuildStatus
    {
        Started, FinishedSuccess, FinishedFailure
    }

    public class ProjectHistoryProjectionView
    {
        readonly IDocumentWriter<ProjectId, ProjectHistory> writer;

        public ProjectHistoryProjectionView(IDocumentWriter<ProjectId, ProjectHistory> writer)
        {
            this.writer = writer;
        }

        public void When(ProjectRegistered ev)
        {
            writer.UpdateEnforcingNew(ev.Id, history =>{});
        }

        public void When(ProjectCheckedOut ev)
        {
            writer.UpdateOrThrow(ev.ProjectId, history =>
                {
                    if (history.CurrentHistory != ProjectBuild.None)
                    {
                        history.Items.Add(history.CurrentHistory);
                    }
                    history.CurrentHistory = new ProjectBuild();
                    history.Current = new Build();
                });
        }

        public void When(TerminalUpdated ev)
        {
            writer.UpdateOrThrow(ev.ProjectId, history => BuildProjection.OnTerminalOutput(ev, history.Current));
        }
        public void When(BuildUpdated ev)
        {
            writer.UpdateOrThrow(ev.ProjectId, history => BuildProjection.OnBuildUpdated(ev, history.Current));
        }

        public void When(BuildStarted ev)
        {
            writer.UpdateOrThrow(ev.ProjectId, history => BuildProjection.OnBuildStarted(ev, history.Current));
        }

        public void When(BuildEnded ev)
        {
            writer.UpdateOrThrow(ev.ProjectId, history =>
                {
                    BuildProjection.OnBuildEnded(ev, history.Current);
                    history.Items.Single(build => build.BuildId == ev.Id)
                           .Status = (ev.Status == BuildTotalEndStatus.Success)
                                         ? BuildStatus.FinishedSuccess
                                         : BuildStatus.FinishedFailure;

                });
        }
    }
}
