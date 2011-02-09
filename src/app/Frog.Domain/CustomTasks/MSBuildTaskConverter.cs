using System.Collections.Generic;
using Frog.Domain.TaskDetection;

namespace Frog.Domain
{
    class MSBuildTaskConverter : TaskConvertor<MSBuildTaskDescriptions>
    {
        public List<ExecTask> GetTasks(MSBuildTaskDescriptions msg)
        {
            return new List<ExecTask>();
        }
    }
}