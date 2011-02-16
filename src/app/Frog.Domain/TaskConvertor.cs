using System.Collections.Generic;
using Frog.Domain.CustomTasks;

namespace Frog.Domain
{
    public interface TaskConvertor<T> where T : ITask
    {
        List<ExecTask> GetTasks(T msg);
    }
}