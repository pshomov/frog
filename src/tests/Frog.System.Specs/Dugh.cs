using System;
using NHamcrest;
using NHamcrest.Core;
using xray;

namespace Frog.System.Specs
{
	public class Dugh
	{
		public void Fle(){
			Take.Snapshot(() => 5).Has(x => x, Is.EqualTo(6));
		}
	}
}

