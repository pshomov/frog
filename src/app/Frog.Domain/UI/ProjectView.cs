using System;
using System.Collections.Concurrent;

namespace Frog.Domain.UI
{
    public class ProjectView
    {
        private readonly ConcurrentDictionary<Guid, BuildStatus> report;

        public ProjectView(ConcurrentDictionary<Guid, BuildStatus> report)
        {
            this.report = report;
        }

        public BuildStatus this[Guid id] { get { return report[id]; } set { report[id] = value; } }
    }
}