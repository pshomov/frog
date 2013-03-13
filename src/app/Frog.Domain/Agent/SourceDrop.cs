namespace Frog.Domain
{
    public class SourceDrop
    {
        public string SourceDropLocation
        {
            get { return sourceDropLocation; }
        }

        public SourceDrop(string sourceDropLocation)
        {
            this.sourceDropLocation = sourceDropLocation;
        }

        readonly string sourceDropLocation;
    }
}