using System;
using System.IO;
using System.Linq;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Frog.Domain.Integration.UI
{
    public class StateBasedTerminalOutputStore : TerminalOutputStore
    {
        public StateBasedTerminalOutputStore(string terminalViewConnection)
        {
            terminal_view_connection = terminalViewConnection;
        }

        public void RegisterTerminalOutput(TerminalUpdate message)
        {
            var mongoServer = MongoServer.Create(terminal_view_connection);
            var db = mongoServer.GetDatabase("terminal_view");
            var terminalOutputs = db.GetCollection<BsonDocument>("terminal_outputs");
            foreach (var i in Enumerable.Range(1, 3))
            {
                if (terminalOutputs.FindOne(new QueryDocument(terminalid_field, message.TerminalId)) == null)
                    terminalOutputs.Insert(new BsonDocument { { terminalid_field, message.TerminalId } });
                else break;
            }
            var updateOp = terminalOutputs.Update(new QueryDocument(terminalid_field, message.TerminalId),
                                                  new UpdateDocument("$push",
                                                                     new BsonDocument("items",
                                                                                      new BsonDocument
                                                                                          {
                                                                                              {
                                                                                                  "sequenceIndex",
                                                                                                  message.ContentSequenceIndex
                                                                                                  },
                                                                                              {"content", message.Content}
                                                                                          })));
            if (updateOp != null && !updateOp.Ok)
                throw new IOException(string.Format("Cannot update terminal view, reason: {0}",
                                                    updateOp.LastErrorMessage));
        }

        const string terminalid_field = "_id";

        readonly string terminal_view_connection;
    }
}