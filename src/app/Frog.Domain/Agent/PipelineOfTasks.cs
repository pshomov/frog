using System;
using System.Collections.Generic;
using Frog.Domain.ExecTasks;

namespace Frog.Domain
{
    public class PipelineOfTasks : Pipeline
    {
        public event Action<BuildTotalEndStatus> OnBuildEnded = status => { };
        public event BuildStartedDelegate OnBuildStarted = status => { };
        public event Action<int, Guid, TaskInfo.TaskStatus> OnBuildUpdated = (i, terminalId, status) => { };
        public event Action<TerminalUpdateInfo> OnTerminalUpdate = info => { };

        public PipelineOfTasks(TaskSource tasksSource, ExecTaskGenerator execTaskGenerator)
        {
            this.tasksSource = tasksSource;
            this.execTaskGenerator = execTaskGenerator;
        }

        public void Process(SourceDrop sourceDrop)
        {
            var execTasks = GenerateTasks(sourceDrop);
            RunTasks(sourceDrop, execTasks);
        }

        readonly TaskSource tasksSource;
        readonly ExecTaskGenerator execTaskGenerator;

        void RunTasks(SourceDrop sourceDrop, List<ExecutableTask> execTasks)
        {
            var execTaskStatus = ExecTaskResult.Status.Success;
            var status = GeneratePipelineStatus(execTasks);
            OnBuildStarted(status);

            for (var i = 0; i < execTasks.Count; i++)
            {
                var execTask = execTasks[i];
                var terminalId = status.Tasks[i].TerminalId;
                OnBuildUpdated(i, terminalId, TaskInfo.TaskStatus.Started);
                var sequneceIndex = 0;
                Action<string> execTaskOnOnTerminalOutputUpdate =
                    s => OnTerminalUpdate(new TerminalUpdateInfo(sequneceIndex++, s, i, terminalId));
                execTask.OnTerminalOutputUpdate += execTaskOnOnTerminalOutputUpdate;
                try
                {
                    execTaskStatus = execTask.Perform(sourceDrop).ExecStatus;
                }
                catch (Exception e)
                {
                    execTaskStatus = ExecTaskResult.Status.Error;
                    execTaskOnOnTerminalOutputUpdate("Runz>> Exception running the task:" + e);
                }
                execTask.OnTerminalOutputUpdate -= execTaskOnOnTerminalOutputUpdate;
                var taskStatus = execTaskStatus == ExecTaskResult.Status.Error
                                     ? TaskInfo.TaskStatus.FinishedError
                                     : TaskInfo.TaskStatus.FinishedSuccess;
                OnBuildUpdated(i, terminalId, taskStatus);
                if (execTaskStatus != ExecTaskResult.Status.Success) break;
            }

            OnBuildEnded(execTaskStatus == ExecTaskResult.Status.Error
                             ? BuildTotalEndStatus.Error
                             : BuildTotalEndStatus.Success);
        }

        List<ExecutableTask> GenerateTasks(SourceDrop sourceDrop)
        {
            var execTasks = new List<ExecutableTask>();
            bool shouldStop;
            foreach (var task in tasksSource.Detect(sourceDrop.SourceDropLocation, out shouldStop))
            {
                execTasks.AddRange(task.GimeTasks(execTaskGenerator));
            }
            return execTasks;
        }

        PipelineStatus GeneratePipelineStatus(List<ExecutableTask> execTasks)
        {
            var pipelineStatus = new PipelineStatus();
            foreach (var execTask in execTasks)
            {
                pipelineStatus.Tasks.Add(new TaskInfo
                                             {
                                                 Name = execTask.Name,
                                                 Status = TaskInfo.TaskStatus.NotStarted,
                                                 TerminalId = Guid.NewGuid()
                                             });
            }
            return pipelineStatus;
        }
    }
}