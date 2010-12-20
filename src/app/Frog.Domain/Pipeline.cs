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

    public class River : Pipeline
    {
        public void Process(SourceDrop sourceDrop)
        {
            throw new NotImplementedException();
        }
    }
}