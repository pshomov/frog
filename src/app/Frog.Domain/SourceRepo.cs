using System;
namespace Frog.Domain
{
	public interface SourceRepo
	{
	    Int64 LatestRevisionNumber { get; }
	}
}

