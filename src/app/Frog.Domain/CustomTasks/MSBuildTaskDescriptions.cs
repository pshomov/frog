namespace Frog.Domain.CustomTasks
{
    public class MSBuildTaskDescriptions : ITask
    {
        public readonly string SolutionFile;

        public MSBuildTaskDescriptions(string solutionFile)
        {
            SolutionFile = solutionFile;
        }
    }
}