using System;
namespace Frog.Domain
{
	public interface MessageBus
	{
	    void PostTopic(string messageClass);
	}
}

