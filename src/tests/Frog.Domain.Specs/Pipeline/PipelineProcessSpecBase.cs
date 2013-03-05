using System;
using Frog.Domain.BuildSystems.Solution;
using Frog.Domain.TaskSources;
using Frog.Specs.Support;
using NSubstitute;

namespace Frog.Domain.Specs.Pipeline
{
    public abstract class PipelineProcessSpecBase : BDD
    {
        protected Domain.Pipeline Pipeline;
        protected IExecTask Task1;
        protected IExecTask Task2;
        protected TaskSource TaskSource;
        protected IExecTaskGenerator ExecTaskGenerator;
        protected BuildStartedDelegate PipelineOnBuildStarted;
        protected Action<BuildTotalEndStatus> PipelineOnBuildEnded;
        protected Action<int, Guid, TaskInfo.TaskStatus> PipelineOnBuildUpdated;
        protected Action<TerminalUpdateInfo> PipelineOnTerminalUpdate;
        protected int counter = 0;

        protected override void Given()
        {
            TaskSource = Substitute.For<TaskSource>();
            ExecTaskGenerator = Substitute.For<IExecTaskGenerator>();
            Pipeline = new PipelineOfTasks(TaskSource, ExecTaskGenerator);
            ObservingEvents();
        }

        void ObservingEvents()
        {
            PipelineOnBuildStarted = Substitute.For<BuildStartedDelegate>();
            PipelineOnBuildEnded = Substitute.For<Action<BuildTotalEndStatus>>();
            PipelineOnBuildUpdated = Substitute.For<Action<int, Guid, TaskInfo.TaskStatus>>();
            PipelineOnTerminalUpdate = Substitute.For<Action<TerminalUpdateInfo>>();
            Pipeline.OnBuildStarted += PipelineOnBuildStarted;
            Pipeline.OnBuildUpdated += PipelineOnBuildUpdated;
            Pipeline.OnBuildEnded += PipelineOnBuildEnded;
            Pipeline.OnTerminalUpdate += PipelineOnTerminalUpdate;
            Pipeline.OnTerminalUpdate += info => { counter++; };
        }
    }
}