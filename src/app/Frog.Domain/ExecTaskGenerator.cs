using System.Collections.Generic;
using Frog.Domain.CustomTasks;
using Frog.Domain.ExecTasks;
using Frog.Domain.TaskSources;

namespace Frog.Domain
{
    public interface IExecTaskGenerator
    {
        List<IExecTask> GimeTasks(ITask task);
    }

    public class ExecTaskGenerator : IExecTaskGenerator
    {
        readonly ExecTaskFactory execTaskGenerator;

        public ExecTaskGenerator(ExecTaskFactory execTaskGenerator)
        {
            this.execTaskGenerator = execTaskGenerator;
        }

        public List<IExecTask> GimeTasks(ITask task)
        {
            var result = new List<IExecTask>();
            if (task.GetType() == typeof (MSBuildTask))
            {
                var mstask = (MSBuildTask) task;
                result.Add(execTaskGenerator.CreateTask("xbuild", mstask.SolutionFile, "build"));
            }
            if (task.GetType() == typeof (NUnitTask))
            {
                var nunit = (NUnitTask) task;
                result.Add(execTaskGenerator.CreateTask("nunit", nunit.Assembly, "unit_test"));
            }
            if(task.GetType() == typeof(TestTaskDescription))
            {
                result.Add(new TestExecTask((task as TestTaskDescription).path));
            }
            if(task.GetType() == typeof(RakeTask))
            {
                result.Add(execTaskGenerator.CreateTask("rake.exe", null, "unit_test"));
            }
            return result;
        }
    }
}