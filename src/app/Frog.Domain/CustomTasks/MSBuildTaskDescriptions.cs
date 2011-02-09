using Frog.Domain.CustomTasks;

namespace Frog.Domain.TaskDetection
{
    public class MSBuildTaskDescriptions : ITask
    {
        public readonly string solutionFile;

        public MSBuildTaskDescriptions(string solutionFile)
        {
            this.solutionFile = solutionFile;
        }
    }
}