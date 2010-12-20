using System;
using System.Collections.Generic;

namespace Frog.Domain
{
    public class PipelineRepository
    {
        readonly List<Pipeline> streams;

        public PipelineRepository(params Pipeline[] pipeline)
        {
            streams = new List<Pipeline>(pipeline);
        }

        public void SourceUpdate(string waterID, int revision, string path)
        {
            streams.ForEach(stream1 => stream1.Process(new SourceDrop(waterID, revision, path)));
        }
    }
}