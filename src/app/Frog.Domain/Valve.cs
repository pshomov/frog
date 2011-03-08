using System;
using SimpleCQRS;

namespace Frog.Domain
{
    public interface IValve
    {
        void Check(SourceRepoDriver repoUrl, string revision);
    }

    public class UpdateFound : Event
    {
        public string BuildId;
        public string Revision;
    }

    public class Valve : IValve
    {
        readonly Pipeline _pipeline;
        readonly WorkingArea _workingArea;
        readonly IEventPublisher eventPublisher;

        public Valve(Pipeline pipeline, WorkingArea workingArea, IEventPublisher eventPublisher)
        {
            _pipeline = pipeline;
            _workingArea = workingArea;
            this.eventPublisher = eventPublisher;
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