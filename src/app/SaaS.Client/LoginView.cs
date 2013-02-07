using System;
using System.Runtime.Serialization;
using Lokad.Cqrs;
using Lokad.Cqrs.AtomicStorage;
using SaaS.Engine;

namespace SaaS.Client
{
    [DataContract]
    public sealed class BuildHistoryView
    {
        [DataMember(Order = 1)]
        public string Display { get; set; }

        [DataMember(Order = 2)]
        public string Display2 { get; set; }

        public BuildHistoryView()
        {
        }
    }

    public sealed class BuildHistoryViewProjection
    {
        readonly IDocumentWriter<unit, BuildHistoryView> writer;

        public BuildHistoryViewProjection(IDocumentWriter<unit, BuildHistoryView> writer)
        {
            this.writer = writer;
        }

        public void When(BuildStarted e)
        {
            Console.WriteLine("Got "+e);
        }

    }

}