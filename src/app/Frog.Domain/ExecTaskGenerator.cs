using System.Collections.Generic;
using Frog.Domain.CustomTasks;

namespace Frog.Domain
{
    public interface IExecTaskGenerator
    {
        List<ExecTask> GimeTasks(ITask task);
    }

    public class ExecTaskGenerator : IExecTaskGenerator
    {
        readonly ExecTaskFactory execTaskGenerator;

        public ExecTaskGenerator(ExecTaskFactory execTaskGenerator)
        {
            this.execTaskGenerator = execTaskGenerator;
        }

        public List<ExecTask> GimeTasks(ITask task)
        {
            var result = new List<ExecTask>();
            if (task.GetType() == typeof (MSBuildTaskDescriptions))
            {
                var mstask = (MSBuildTaskDescriptions) task;
                result.Add(execTaskGenerator.CreateTask("xbuild", mstask.SolutionFile, "build"));
            }
            if (task.GetType() == typeof (NUnitTask))
            {
                var nunit = (NUnitTask) task;
                result.Add(execTaskGenerator.CreateTask("nunit", nunit.Assembly, "unit_test"));
            }
            return result;
        }
    }
}