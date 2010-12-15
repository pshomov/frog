namespace Frog.Domain
{
    public interface Stream
    {
        void Water(string waterID, int revision, string path);
    }
}