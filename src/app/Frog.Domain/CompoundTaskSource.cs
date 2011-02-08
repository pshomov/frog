using System.Collections.Generic;
using Frog.Domain.CustomTasks;

namespace Frog.Domain
{
    class CompoundTaskSource : TaskSource
    {
        readonly TaskSource[] srcs;

        public CompoundTaskSource(params TaskSource[] srcs)
        {
            this.srcs = srcs;
        }

        public IList<ITask> Detect()
        {
            var result = new List<ITask>(0);
            foreach (var taskSource in srcs)
            {
                result.AddRange(taskSource.Detect());
            }
            return result;
        }
    }
}