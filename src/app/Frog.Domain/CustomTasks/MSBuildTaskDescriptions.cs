using Frog.Domain.CustomTasks;

namespace Frog.Domain.TaskDetection
{
    public class MSBuildTaskDescriptions : TaskBase
    {
        public readonly string solutionFile;

        public MSBuildTaskDescriptions(string solutionFile)
        {
            this.solutionFile = solutionFile;
        }
    }
}