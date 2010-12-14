using System;
namespace Frog.Domain
{
	public class SourceFountain
	{
		MessageBus bus;
		SourceRepo repo;
	    private Int64 currentRevisionNumber;
		
		public SourceFountain(SourceRepo repo, MessageBus bus){
			this.bus = bus;
			this.repo = repo;
		    currentRevisionNumber = Int64.MinValue;
		}

	    public void HasSplash()
	    {
	        var latestRevisionNumber = repo.LatestRevisionNumber;
	        if (latestRevisionNumber != currentRevisionNumber)
            {
                currentRevisionNumber = latestRevisionNumber;
                bus.PostTopic("src_new_revision");
            }
	    }
	}
}

