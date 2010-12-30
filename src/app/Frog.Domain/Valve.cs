namespace Frog.Domain
{
    public class Valve
    {
        readonly SourceRepoDriver _sourceRepoDriver;
        readonly Pipeline _pipeline;
        readonly WorkingArea _workingArea;

        public Valve(SourceRepoDriver sourceRepoDriver, Pipeline pipeline, WorkingArea workingArea)
        {
            _sourceRepoDriver = sourceRepoDriver;
            _pipeline = pipeline;
            _workingArea = workingArea;
        }

        public void Check()
        {
            if (_sourceRepoDriver.CheckForUpdates())
            {
                var sourceDropLocation = _workingArea.AllocateWorkingArea();
                var sourceDrop = _sourceRepoDriver.GetLatestSourceDrop(sourceDropLocation);
                _pipeline.Process(sourceDrop);
            }
        }
    }

    public interface WorkingArea
    {
        string AllocateWorkingArea();
    }
}