using System.Collections.Generic;
using Frog.Domain.CustomTasks;

namespace Frog.Domain.TaskSources
{
    public class CompoundTaskSource : TaskSource
    {
        readonly TaskSource[] srcs;

        public CompoundTaskSource(params TaskSource[] srcs)
        {
            this.srcs = srcs;
        }

        public IList<ITask> Detect(string projectFolder)
        {
            var result = new List<ITask>();
            foreach (var taskSource in srcs)
            {
                result.AddRange(taskSource.Detect(projectFolder));
            }
            return result;
        }
    }
}