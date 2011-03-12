using System;
using System.Collections.Generic;
using Frog.Domain;
using Frog.Domain.UI;

namespace Frog.UI.Web
{
    public static class ServiceLocator
    {
        public static Dictionary<string, PipelineStatusView.BuildStatus> Report { get; set; }

        public static RepositoryTracker RepositoryTracker { get; set; }
    }
}