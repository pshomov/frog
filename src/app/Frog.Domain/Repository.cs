using System;
namespace Frog.Domain
{
	public class Repository
	{
		MessageBus bus;
		SourceRepositoryDriver _repositoryDriverDriver;
	    private Int64 currentRevisionNumber;
		
		public Repository(SourceRepositoryDriver repositoryDriver, MessageBus bus){
			this.bus = bus;
			this._repositoryDriverDriver = repositoryDriver;
		    currentRevisionNumber = Int64.MinValue;
		}

	    public void CheckForUpdates()
	    {
	        var latestRevisionNumber = _repositoryDriverDriver.LatestRevisionNumber;
	        if (latestRevisionNumber != currentRevisionNumber)
            {
                currentRevisionNumber = latestRevisionNumber;
                bus.PostTopic("src_new_revision");
            }
	    }
	}
}

