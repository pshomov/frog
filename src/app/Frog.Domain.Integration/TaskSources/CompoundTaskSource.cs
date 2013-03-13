using System.Collections.Generic;
using Frog.Domain.TaskSources;

namespace Frog.Domain.Integration.TaskSources
{
    public class CompoundTaskSource : TaskSource
    {
        public CompoundTaskSource(params TaskSource[] srcs)
        {
            this.srcs = srcs;
        }

        public IEnumerable<TaskDescription> Detect(string projectFolder, out bool shouldStop)
        {
            shouldStop = false;
            var result = new List<TaskDescription>();
            foreach (var taskSource in srcs)
            {
                bool taskSaysShouldStop;
                result.AddRange(taskSource.Detect(projectFolder, out taskSaysShouldStop));
                if (taskSaysShouldStop) break;
            }
            return result;
        }

        readonly TaskSource[] srcs;
    }
}