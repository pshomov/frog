using System;
using System.Collections.Generic;
using Frog.Domain.ExecTasks;
using Frog.Domain.TaskSources;

namespace Frog.Domain
{
    public class PipelineOfTasks : Pipeline
    {
        readonly TaskSource tasksSource;
        readonly IExecTaskGenerator execTaskGenerator;

        public PipelineOfTasks(TaskSource tasksSource, IExecTaskGenerator execTaskGenerator)
        {
            this.tasksSource = tasksSource;
            this.execTaskGenerator = execTaskGenerator;
        }

        public void Process(SourceDrop sourceDrop)
        {
            var execTasks = GenerateTasks(sourceDrop);
            RunTasks(sourceDrop, execTasks);
        }

        public event BuildStartedDelegate OnBuildStarted = status => {};
        public event Action<int, Guid, TaskInfo.TaskStatus> OnBuildUpdated = (i, terminalId, status) => {};
        public event Action<BuildTotalEndStatus> OnBuildEnded = status => {};
        public event Action<TerminalUpdateInfo> OnTerminalUpdate = info => {};

        void RunTasks(SourceDrop sourceDrop, List<IExecTask> execTasks)
        {
            ExecTaskResult.Status execTaskStatus = ExecTaskResult.Status.Success;
            PipelineStatus status = GeneratePipelineStatus(execTasks);
            OnBuildStarted(status);

            for (int i = 0; i < execTasks.Count; i++)
            {
                var execTask = execTasks[i];
                var terminalId = status.Tasks[i].TerminalId;
                OnBuildUpdated(i, terminalId, TaskInfo.TaskStatus.Started);
                int sequneceIndex = 0;
                Action<string> execTaskOnOnTerminalOutputUpdate = s => OnTerminalUpdate(new TerminalUpdateInfo(sequneceIndex++, s, i, terminalId));
                execTask.OnTerminalOutputUpdate += execTaskOnOnTerminalOutputUpdate;
                try
                {
                    execTaskStatus = execTask.Perform(sourceDrop).ExecStatus;
                }
                catch(Exception e)
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

        List<IExecTask> GenerateTasks(SourceDrop sourceDrop)
        {
            var execTasks = new List<IExecTask>();
            foreach (var task in tasksSource.Detect(sourceDrop.SourceDropLocation))
            {
                execTasks.AddRange(execTaskGenerator.GimeTasks(task));
            }
            return execTasks;
        }

        PipelineStatus GeneratePipelineStatus(List<IExecTask> execTasks)
        {
            var pipelineStatus = new PipelineStatus();
            foreach (var execTask in execTasks)
            {
                pipelineStatus.Tasks.Add(new TaskInfo
                                             {Name = execTask.Name, Status = TaskInfo.TaskStatus.NotStarted, TerminalId = Guid.NewGuid()});
            }
            return pipelineStatus;
        }
    }
}