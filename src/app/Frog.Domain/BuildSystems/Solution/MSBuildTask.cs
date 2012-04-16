namespace Frog.Domain.BuildSystems.Solution
{
    public class MSBuildTask : Task
    {
        public readonly string SolutionFile;

        public MSBuildTask(string solutionFile)
        {
            SolutionFile = solutionFile;
        }
    }
}