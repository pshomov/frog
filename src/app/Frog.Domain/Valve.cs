using Frog.Domain.SourceRepositories;

namespace Frog.Domain
{
    public class Valve
    {
        readonly SourceRepoDriver _sourceRepoDriver;
        readonly Pipeline _pipeline;

        public Valve(SourceRepoDriver sourceRepoDriver, Pipeline pipeline)
        {
            _sourceRepoDriver = sourceRepoDriver;
            _pipeline = pipeline;
        }

        public void Check()
        {
            if (_sourceRepoDriver.CheckForUpdates())
                _pipeline.Process(null);
        }
    }
}