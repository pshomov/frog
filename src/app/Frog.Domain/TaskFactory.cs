using System.Collections.Generic;
using Frog.Domain.Specs;

namespace Frog.Domain
{
    public class TaskFactory
    {
        readonly TaskDetector[] taskDetectors;

        public TaskFactory(params TaskDetector[] taskDetectors)
        {
            this.taskDetectors = taskDetectors;
        }

        public IList<ExecTask> Generate()
        {
            return taskDetectors[0].Detect();
        }
    }
}