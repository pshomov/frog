using System;
using Machine.Specifications;
using Rhino.Mocks;

namespace Frog.Domain.Specs
{
	public class SourceFountaint_Normal_Behaviour
	{
		public static SourceFountain fountain;
		public static MessageBus bus = MockRepository.GenerateMock<MessageBus>();
		public static SourceRepo repo = MockRepository.GenerateMock<SourceRepo>();
		
		Because of = () => fountain = new SourceFountain(repo, bus);
		It should_do_something_cool = () => fountain.ShouldNotBeNull();
	}
}

