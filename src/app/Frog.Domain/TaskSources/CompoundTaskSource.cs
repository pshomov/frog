using System.Collections.Generic;

namespace Frog.Domain.TaskSources
{
    public class CompoundTaskSource : TaskSource
    {
        public CompoundTaskSource(params TaskSource[] srcs)
        {
            this.srcs = srcs;
        }

        public IList<Task> Detect(string projectFolder)
        {
            var result = new List<Task>();
            foreach (var taskSource in srcs)
            {
                result.AddRange(taskSource.Detect(projectFolder));
            }
            return result;
        }

        readonly TaskSource[] srcs;
    }
}