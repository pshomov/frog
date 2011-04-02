using System.Collections.Concurrent;
using Frog.Domain;
using Frog.Domain.UI;

namespace Frog.UI.Web
{
    public static class ServiceLocator
    {
        public static ConcurrentDictionary<string, BuildStatus> Report { get; set; }

        public static RepositoryTracker RepositoryTracker { get; set; }
    }
}