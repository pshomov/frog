using System;
using Frog.Domain.ExecTasks;

namespace Frog.Domain
{
    public interface ExecutableTask
    {
        string Name { get; }
        event Action<string> OnTerminalOutputUpdate;
        ExecTaskResult Perform(SourceDrop sourceDrop);
    }
}