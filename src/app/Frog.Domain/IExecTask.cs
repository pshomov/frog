using System;
using Frog.Domain.ExecTasks;

namespace Frog.Domain
{
    public interface IExecTask
    {
        event Action<string> OnTerminalOutputUpdate;
        string Name { get; }
        ExecTaskResult Perform(SourceDrop sourceDrop);
    }
}