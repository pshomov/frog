using System;
namespace Frog.Domain
{
	public class Repository
	{
		MessageBus bus;
		SourceRepo repoDriver;
	    private Int64 currentRevisionNumber;
		
		public Repository(SourceRepo repo, MessageBus bus){
			this.bus = bus;
			this.repoDriver = repo;
		    currentRevisionNumber = Int64.MinValue;
		}

	    public void CheckForUpdates()
	    {
	        var latestRevisionNumber = repoDriver.LatestRevisionNumber;
	        if (latestRevisionNumber != currentRevisionNumber)
            {
                currentRevisionNumber = latestRevisionNumber;
                bus.PostTopic("src_new_revision");
            }
	    }
	}
}

