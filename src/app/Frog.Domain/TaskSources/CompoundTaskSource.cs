using System;
using System.Collections.Generic;

namespace Frog.Domain.TaskSources
{
    public class CompoundTaskSource : TaskSource
    {
        public CompoundTaskSource(params TaskSource[] srcs)
        {
            this.srcs = srcs;
        }

        public IList<Task> Detect(string projectFolder, Func<string, string> getContent)
        {
            var result = new List<Task>();
            foreach (var taskSource in srcs)
            {
                result.AddRange(taskSource.Detect(projectFolder, null));
            }
            return result;
        }

        readonly TaskSource[] srcs;
    }
}