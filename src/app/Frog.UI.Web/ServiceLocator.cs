using System;
using Frog.Domain;

namespace Frog.UI.Web
{
    public static class ServiceLocator
    {
        public static PipelineStatusView.BuildStatus Report { get; set; }

        public static Valve Valve { get; set; }
    }
}