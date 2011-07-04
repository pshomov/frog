using Frog.Specs.Support;
using NSubstitute;

namespace Frog.Domain.Specs.TaskDetection
{
    public abstract class TaskDetectorSpecsBase : BDD
    {
        protected TaskFileFinder taskFileFinder;

        protected override void SetupDependencies()
        {
            taskFileFinder = Substitute.For<TaskFileFinder>();
        }
    }
}