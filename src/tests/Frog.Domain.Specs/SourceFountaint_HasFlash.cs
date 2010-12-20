using System;
using Machine.Specifications;
using Rhino.Mocks;

namespace Frog.Domain.Specs
{
    public class Repository_First_Run : RepositorySpec
    {
        Establish context = () => repository = new Repository(repo, bus);
        Because of = () => repository.CheckForUpdates();
        It should_check_source_repo = () => repo.AssertWasCalled(x => x.LatestRevisionNumber);
        It should_send_message = () => bus.AssertWasCalled(x => x.PostTopic("src_new_revision"));
    }

    public class Repository_Later_Run : RepositorySpec
    {
        private Establish context = () =>
                                        {
                                            repository = new Repository(repo, bus);
                                            repo.Stub(x => x.LatestRevisionNumber).Return(1).Return(2);
                                            repository.CheckForUpdates();
                                        };
        Because of = () => repository.CheckForUpdates();
        It should_check_source_repo_twice = () => repo.AssertWasCalled(x => x.LatestRevisionNumber, options => options.Repeat.Twice());
        It should_send_message = () => bus.AssertWasCalled(x => x.PostTopic("src_new_revision"));
    }

    public class Repository_No_Change_In_Src_Revision_Run : RepositorySpec
    {
        private Establish context = () =>
                                        {
                                            repository = new Repository(repo, bus);
                                            repo.Stub(x => x.LatestRevisionNumber).Return(1).Repeat.Any();
                                            repository.CheckForUpdates();
                                        };

        Because of = () => repository.CheckForUpdates();
        It should_not_send_message = () => bus.AssertWasCalled(x => x.PostTopic("src_new_revision"), options => options.Repeat.Once());
    }

    public class RepositorySpec
    {
        public static Repository repository;
        public static MessageBus bus;
        public static SourceRepo repo;

        Establish context = () =>
                                {
                                    bus = MockRepository.GenerateMock<MessageBus>();
                                    repo = MockRepository.GenerateMock<SourceRepo>();
                                };
    }
}

