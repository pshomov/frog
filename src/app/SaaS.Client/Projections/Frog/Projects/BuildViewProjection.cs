using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.Serialization;
using Lokad.Cqrs;
using Lokad.Cqrs.AtomicStorage;
using SaaS.Engine;

namespace SaaS.Client.Projections.Releases
{
    [DataContract]
    public sealed class Build
    {
        public enum BuildOverallStatus
        {
            NotStarted, Started, EndedSuccess, EndedFailure
        }
        [DataMember(Order = 1)]
        public List<TaskInfo> Tasks { get; set; }
        [DataMember(Order = 2)]
        public Dictionary<TerminalId, List<string>> TerminalOutput { get; set; }
        [DataMember(Order = 3)]
        public BuildOverallStatus Status;

        public Build()
        {
            Tasks = new List<TaskInfo>();
            TerminalOutput = new Dictionary<TerminalId, List<string>>();
            Status = BuildOverallStatus.NotStarted;
        }
    }



    public class BuildViewProjection
    {
        readonly IDocumentWriter<BuildId, Build> writer;

        public BuildViewProjection(IDocumentWriter<BuildId, Build> writer)
        {
            this.writer = writer;
        }

        public void When(BuildStarted e)
        {
            writer.UpdateEnforcingNew(e.Id, view =>
                {
                    view.Tasks = new List<TaskInfo>(e.Status.Tasks);
                    view.Tasks.ForEach(info => view.TerminalOutput[info.Id] = new List<string>());
                    view.Status = Build.BuildOverallStatus.Started;
                });
        }

        public void When(BuildUpdated e)
        {
            writer.UpdateOrThrow(e.Id, view => view.Tasks[e.TaskIndex].Status = e.TaskStatus);
        }

        public void When(TerminalUpdated e)
        {
            writer.UpdateOrThrow(e.BuildId, view =>
                {
                    Contract.Requires(e.ContentSequnceIndex == view.TerminalOutput[e.Id].Count);
                    view.TerminalOutput[e.Id].Add(e.Content);
                });
        }

        public void When(BuildEnded e)
        {
            writer.UpdateOrThrow(e.Id, view => view.Status = e.Status == BuildTotalEndStatus.Error ? Build.BuildOverallStatus.EndedFailure : Build.BuildOverallStatus.EndedSuccess);
        }
    }
}