using SimpleCQRS;

namespace Frog.Domain
{
    public class CheckForUpdateFailed : Event
    {
        public string repoUrl { get; set; }
    }
}