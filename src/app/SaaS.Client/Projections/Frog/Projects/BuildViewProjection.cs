using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.Serialization;
using Lokad.Cqrs.AtomicStorage;
using SaaS.Engine;

namespace SaaS.Client.Projections.Frog.Projects
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

        public static Build None = new Build();

        public Build()
        {
            Tasks = new List<TaskInfo>();
            TerminalOutput = new Dictionary<TerminalId, List<string>>();
            Status = BuildOverallStatus.NotStarted;
        }

        public string GetTerminalOutput(TerminalId terminalId)
        {
            if (!TerminalOutput.ContainsKey(terminalId)) return "";
            var terminalOutput = TerminalOutput[terminalId].Skip(1).Aggregate((s, s1) => s + s1+ "\r\n");
            return terminalOutput;
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
            writer.UpdateEnforcingNew(e.Id, view => BuildProjection.OnBuildStarted(e, view));
        }

        public void When(BuildUpdated e)
        {
            writer.UpdateOrThrow(e.Id, view => BuildProjection.OnBuildUpdated(e, view));
        }

        public void When(TerminalUpdated e)
        {
            writer.UpdateOrThrow(e.BuildId, view => BuildProjection.OnTerminalOutput(e, view));
        }

        public void When(BuildEnded e)
        {
            writer.UpdateOrThrow(e.Id, view => BuildProjection.OnBuildEnded(e, view));
        }
    }
}