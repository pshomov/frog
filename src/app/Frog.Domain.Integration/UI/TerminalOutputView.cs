using System;
using System.IO;
using System.Linq;
using System.Text;
using EventStore;
using MongoDB.Bson;
using MongoDB.Driver;
using SimpleCQRS;

namespace Frog.Domain.Integration.UI
{
    public class TerminalOutputView : ViewForTerminalOutput
    {
        const string terminalid_field = "_id";
        readonly string connection;

        public TerminalOutputView(string connection)
        {
            this.connection = connection;
        }

        public TerminalOutput.Info GetTerminalOutput(Guid terminalId, int sinceIndex)
        {
            var mongoServer = MongoServer.Create(connection);
            var db = mongoServer.GetDatabase("terminal_view");
            var terminalOutputs = db.GetCollection<BsonDocument>("terminal_outputs");
            var doc = terminalOutputs.FindOne(new QueryDocument(terminalid_field, terminalId));
            var items = doc["items"].AsBsonArray.ToList().Where(value => value.AsBsonDocument["sequenceIndex"].AsInt32 > sinceIndex)
                .OrderByDescending(value => value.AsBsonDocument["sequenceIndex"].AsInt32).ToList();
            var content = new StringBuilder();
            items.ForEach(value => content.Append(value.AsBsonDocument["content"].AsString));
            return new TerminalOutput.Info(){Content = content.ToString(), LastChunkIndex = items.Last().AsBsonDocument["sequenceIndex"].AsInt32};
        }

        public string GetTerminalOutput(Guid terminalId)
        {
            var mongoServer = MongoServer.Create(connection);
            var db = mongoServer.GetDatabase("terminal_view");
            var terminalOutputs = db.GetCollection<BsonDocument>("terminal_outputs");
            var doc = terminalOutputs.FindOne(new QueryDocument(terminalid_field, terminalId));
            if (doc == null) return "";
            var items = doc["items"].AsBsonArray.ToList()
                .OrderBy(value => value.AsBsonDocument["sequenceIndex"].AsInt32).ToList();
            var content = new StringBuilder();
            items.ForEach(value => content.Append(value.AsBsonDocument["content"].AsString));
            return content.ToString();
        }
    }

    public class TerminalOutputA : AggregateRoot
    {
        public override Guid Id
        {
            get { return terminalId; }
        }

        public string Value
        {
            get { return terminalOutput.GetContent(0).Content; }
        }

        public TerminalOutputA(Guid terminalId)
        {
            this.terminalId = terminalId;
            terminalOutput = new TerminalOutput();
        }

        public TerminalOutput.Info Info(int sinceIndex)
        {
            return terminalOutput.GetContent(sinceIndex);
        }

        readonly Guid terminalId;
        readonly TerminalOutput terminalOutput;

        void Apply(TerminalUpdate msg)
        {
            terminalOutput.Add(msg.ContentSequenceIndex, msg.Content);
        }
    }
}