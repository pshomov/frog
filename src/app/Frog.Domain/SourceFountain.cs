using System;
namespace Frog.Domain
{
	public class SourceFountain
	{
		MessageBus bus;
		SourceRepo repo;
		
		public SourceFountain(SourceRepo repo, MessageBus bus){
			this.bus = bus;
			this.repo = repo;
		}
	}
}

