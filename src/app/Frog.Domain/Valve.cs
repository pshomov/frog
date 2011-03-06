using System;
using SimpleCQRS;

namespace Frog.Domain
{
    public interface IValve
    {
        void Check();
        void Check(SourceRepoDriver repoUrl, string revision);
    }

    public class UpdateFound : Event
    {
        public string BuildId;
        public string Revision;
    }

    public class Valve : IValve
    {
        readonly SourceRepoDriver _sourceRepoDriver;
        readonly Pipeline _pipeline;
        readonly WorkingArea _workingArea;
        readonly IEventPublisher eventPublisher;

        public Valve(SourceRepoDriver sourceRepoDriver, Pipeline pipeline, WorkingArea workingArea, IEventPublisher eventPublisher = null)
        {
            _sourceRepoDriver = sourceRepoDriver;
            _pipeline = pipeline;
            _workingArea = workingArea;
            this.eventPublisher = eventPublisher;
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

        public void Check(SourceRepoDriver repoUrl, string revision)
        {
            string latestRev;
            if ((latestRev = repoUrl.GetLatestRevision()) != revision)
            {
                eventPublisher.Publish(new UpdateFound{Revision = latestRev});
                var workingArea = _workingArea.AllocateWorkingArea();
                repoUrl.GetSourceRevision(latestRev, workingArea);
                _pipeline.Process(new SourceDrop(workingArea));
            }
        }
    }

    public interface WorkingArea
    {
        string AllocateWorkingArea();
    }
}