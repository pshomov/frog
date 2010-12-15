using System;
using System.Collections.Generic;

namespace Frog.Domain
{
    public class StreamsRepository
    {
        readonly List<Stream> streams;

        public StreamsRepository(params Stream[] stream)
        {
            streams = new List<Stream>(stream);
        }

        public void Water(string waterID, int revision, string path)
        {
            streams.ForEach(stream1 => stream1.Water(waterID, revision, path));
        }
    }
}