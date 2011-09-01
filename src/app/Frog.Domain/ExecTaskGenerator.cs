using System.Collections.Generic;
using Frog.Domain.BuildSystems.FrogSystemTest;
using Frog.Domain.BuildSystems.Rake;
using Frog.Domain.BuildSystems.Solution;
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
            if (task.GetType() == typeof(TestTaskDescription))
            {
                result.Add(new TestExecTask((task as TestTaskDescription).path, this));
            }
            if (task.GetType() == typeof(FakeTaskDescription))
            {
                result.Add(new FakeExecTask((task as FakeTaskDescription).messages, this));
            }
            if (task.GetType() == typeof(RakeTask))
            {
                result.Add(execTaskGenerator.CreateTask("rvm", "use 1.9.2", "prepare_for_rake"));
                result.Add(execTaskGenerator.CreateTask("rake", null, "unit_test"));
            }
            if(task.GetType() == typeof(BundlerTask))
            {
                result.Add(execTaskGenerator.CreateTask("bundle", null, "Bundler"));
            }
            if(task.GetType() == typeof(AnyTask))
            {
                var anyTask = task as AnyTask;
                result.Add(execTaskGenerator.CreateTask(anyTask.cmd, anyTask.args, "System Task"));
            }
            return result;
        }
    }
}