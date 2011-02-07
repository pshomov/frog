namespace Frog.Domain.TaskDetection
{
    public struct MSBuildTaskDescriptions
    {
        public readonly string solutionFile;

        public MSBuildTaskDescriptions(string solutionFile)
        {
            this.solutionFile = solutionFile;
        }
    }
}