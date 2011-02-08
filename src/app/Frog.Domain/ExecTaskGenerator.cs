using System.Collections.Generic;
using Frog.Domain.CustomTasks;
using Frog.Domain.TaskDetection;

namespace Frog.Domain
{
    public class ExecTaskGenerator : TaskDispenser
    {
        readonly TaskSource iTaskProvider;
        readonly ExecTaskFactory execTaskGenerator;

        public ExecTaskGenerator(TaskSource iTaskProvider, ExecTaskFactory execTaskGenerator)
        {
            this.iTaskProvider = iTaskProvider;
            this.execTaskGenerator = execTaskGenerator;
        }


        public List<ExecTask> GimeTasks()
        {
            var result = new List<ExecTask>();
            foreach (var task in iTaskProvider.Detect())
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