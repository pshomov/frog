using System.Collections.Generic;
using Frog.Domain.CustomTasks;

namespace Frog.Domain
{
    public interface TaskSource
    {
        IList<ITask> Detect(string projectFolder);
    }
}