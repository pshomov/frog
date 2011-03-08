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
        readonly Pipeline pipeline;
        readonly WorkingArea workingArea;
        readonly IEventPublisher eventPublisher;

        public Valve(Pipeline pipeline, WorkingArea workingArea, IEventPublisher eventPublisher)
        {
            this.pipeline = pipeline;
            this.workingArea = workingArea;
            this.eventPublisher = eventPublisher;
        }

        public void Check(SourceRepoDriver repoUrl, string revision)
        {
            string latestRev;
            if ((latestRev = repoUrl.GetLatestRevision()) != revision)
            {
                eventPublisher.Publish(new UpdateFound {Revision = latestRev});
                var allocatedWorkingArea = workingArea.AllocateWorkingArea();
                repoUrl.GetSourceRevision(latestRev, allocatedWorkingArea);
                pipeline.Process(new SourceDrop(allocatedWorkingArea));
            }
        }
    }

    public interface WorkingArea
    {
        string AllocateWorkingArea();
    }
}