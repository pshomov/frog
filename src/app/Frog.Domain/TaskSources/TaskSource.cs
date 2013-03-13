using System.Collections.Generic;

namespace Frog.Domain.TaskSources
{
    public interface TaskSource
    {
        IEnumerable<TaskDescription> Detect(string projectFolder, out bool shouldStop);
    }
}