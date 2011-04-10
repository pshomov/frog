using Frog.Specs.Support;
using NSubstitute;

namespace Frog.Domain.Specs.TaskDetection
{
    public abstract class TaskDetectorSpecsBase : BDD
    {
        protected TaskFileFinder _taskFileFinder;

        protected override void SetupDependencies()
        {
            _taskFileFinder = Substitute.For<TaskFileFinder>();
        }
    }
}