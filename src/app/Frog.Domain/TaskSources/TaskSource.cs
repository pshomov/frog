using System.Collections.Generic;

namespace Frog.Domain.TaskSources
{
    public interface TaskSource
    {
        IList<Task> Detect(string projectFolder);
    }
}