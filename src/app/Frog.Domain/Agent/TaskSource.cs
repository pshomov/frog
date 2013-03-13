using System.Collections.Generic;

namespace Frog.Domain
{
    public interface TaskSource
    {
        IEnumerable<TaskDescription> Detect(string projectFolder, out bool shouldStop);
    }
}