using System.Collections.Generic;
using Frog.Domain.CustomTasks;
using Frog.Domain.TaskDetection;

namespace Frog.Domain
{
    public class ExecTaskGenerator : TaskDispenser
    {
        readonly TaskSource taskProvider;
        readonly ExecTaskFactory execTaskGenerator;

        public ExecTaskGenerator(TaskSource taskProvider, ExecTaskFactory execTaskGenerator)
        {
            this.taskProvider = taskProvider;
            this.execTaskGenerator = execTaskGenerator;
        }


        public List<ExecTask> GimeTasks(string projectFolder)
        {
            var result = new List<ExecTask>();
            foreach (var task in taskProvider.Detect(projectFolder))
            {
                if (task.GetType() == typeof(MSBuildTaskDescriptions))
                {
                    var mstask = (MSBuildTaskDescriptions) task;
                    result.Add( execTaskGenerator.CreateTask("xbuild", mstask.solutionFile));
                }
                if (task.GetType() == typeof(NUnitTask))
                {
                    var nunit = (NUnitTask) task;
                    result.Add(execTaskGenerator.CreateTask("nunit", nunit.Assembly));
                }
            }
            return result;
        }
    }
}