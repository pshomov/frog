using System.Collections.Generic;

namespace Frog.Domain.TaskSources
{
    public interface TaskSource
    {
        IEnumerable<Task> Detect(string projectFolder, out bool shouldStop);
    }
}