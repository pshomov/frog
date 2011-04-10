using Frog.Domain.CustomTasks;

namespace Frog.Domain.BuildSystems.Solution
{
    public class MSBuildTask : ITask
    {
        public readonly string SolutionFile;

        public MSBuildTask(string solutionFile)
        {
            SolutionFile = solutionFile;
        }
    }
}