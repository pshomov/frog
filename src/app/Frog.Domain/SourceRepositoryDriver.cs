using System;
namespace Frog.Domain
{
	public interface SourceRepositoryDriver
	{
	    Int64 LatestRevisionNumber { get; }
	}
}

