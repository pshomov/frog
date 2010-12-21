using System;

namespace Frog.Domain
{
    public class SourceDrop
    {
        private string _sourceId;
        private int _revision;
        private string _path;

        public SourceDrop(string sourceId, int revision, string path)
        {
            _sourceId = sourceId;
            _revision = revision;
            _path = path;
        }

        public string SourceID
        {
            get { return _sourceId; }
        }

        public int Revision
        {
            get { return _revision; }
        }

        public string Path
        {
            get { return _path; }
        }
    }

    public interface Pipeline
    {
        void Process(SourceDrop sourceDrop);
    }

    public class PipelineOfTasks : Pipeline
    {
        private readonly Task[] _tasks;

        public PipelineOfTasks(params Task[] tasks)
        {
            _tasks = tasks;
        }

        public void Process(SourceDrop sourceDrop)
        {
            _tasks[0].Perform(sourceDrop);
        }
    }
}