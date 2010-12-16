using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Frog.Domain;

namespace Frog.System.Specs
{
    public class TestDriver
    {
        public SourceFountain SourceFontain;
        public FakeStream Stream1;
    }

    public class FakeStream : Stream
    {
        public bool GotWater;

        public void Water(string waterID, int revision, string path)
        {
            GotWater = true;
        }
    }
}
