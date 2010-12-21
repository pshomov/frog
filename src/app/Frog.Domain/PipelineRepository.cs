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

        public void SourceUpdate(SourceDrop sourceDrop)
        {
            streams.ForEach(stream1 => stream1.Process(sourceDrop));
        }
    }
}