namespace Frog.Domain.CustomTasks
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