using System;
using Machine.Specifications;
using Rhino.Mocks;

namespace Frog.Domain.Specs
{
    public class SourceFountaint_First_Run : SourceFountaintSpec
    {
        Establish context = () => fountain = new SourceFountain(repo, bus);
        Because of = () => fountain.HasSplash();
        It should_check_source_repo = () => repo.AssertWasCalled(x => x.LatestRevisionNumber);
        It should_send_message = () => bus.AssertWasCalled(x => x.PostTopic("src_new_revision"));
    }

    public class SourceFountaint_Later_Run : SourceFountaintSpec
    {
        private Establish context = () =>
                                        {
                                            fountain = new SourceFountain(repo, bus);
                                            repo.Stub(x => x.LatestRevisionNumber).Return(1).Return(2);
                                            fountain.HasSplash();
                                        };
        Because of = () => fountain.HasSplash();
        It should_check_source_repo_twice = () => repo.AssertWasCalled(x => x.LatestRevisionNumber, options => options.Repeat.Twice());
        It should_send_message = () => bus.AssertWasCalled(x => x.PostTopic("src_new_revision"));
    }

    public class SourceFountaint_No_Change_In_Src_Revision_Run : SourceFountaintSpec
    {
        private Establish context = () =>
                                        {
                                            fountain = new SourceFountain(repo, bus);
                                            repo.Stub(x => x.LatestRevisionNumber).Return(1).Repeat.Any();
                                            fountain.HasSplash();
                                        };

        Because of = () => fountain.HasSplash();
        It should_not_send_message = () => bus.AssertWasCalled(x => x.PostTopic("src_new_revision"), options => options.Repeat.Once());
    }

    public class SourceFountaintSpec
    {
        public static SourceFountain fountain;
        public static MessageBus bus;
        public static SourceRepo repo;

        Establish context = () =>
                                {
                                    bus = MockRepository.GenerateMock<MessageBus>();
                                    repo = MockRepository.GenerateMock<SourceRepo>();
                                };
    }
}

