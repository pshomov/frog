using System.Collections.Generic;

namespace Frog.Domain
{
    public interface ExecTaskGenerator
    {
        List<ExecutableTask> GimeTasks(ShellTaskDescription taskDescription);
        List<ExecutableTask> GimeTasks(TestTaskDescription taskDescription);
        List<ExecutableTask> GimeTasks(FakeTaskDescription taskDescription);
    }
}