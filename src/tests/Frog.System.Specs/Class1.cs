using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Frog.Domain;

namespace Frog.System.Specs
{
    public class TestDriver
    {
        public Repository SourceFontain;
        public FakePipeline Stream1;
    }

    public class FakePipeline : Pipeline
    {
        public bool GotWater;

        public void Process(SourceDrop sourceDrop)
        {
            GotWater = true;
        }
    }
}
