using System;
using Frog.Domain.CustomTasks;
using Frog.Domain.TaskSources;
using Frog.Specs.Support;
using NSubstitute;

namespace Frog.Domain.Specs.Pipeline
{
    public abstract class PipelineProcessSpecBase : BDD
    {
        protected Domain.Pipeline Pipeline;
        protected ExecTask Task1;
        protected ExecTask Task2;
        protected TaskSource TaskSource;
        protected IExecTaskGenerator ExecTaskGenerator;
        protected MSBuildTaskDescriptions SrcTask1;
        protected Action<PipelineStatus> PipelineOnBuildStarted;
        protected Action<BuildTotalStatus> PipelineOnBuildEnded;
        protected Action<PipelineStatus> PipelineOnBuildUpdated;
        protected Action<TerminalUpdateInfo> PipelineOnTerminalUpdate;

        protected override void Given()
        {
            TaskSource = Substitute.For<TaskSource>();
            ExecTaskGenerator = Substitute.For<IExecTaskGenerator>();
            Pipeline = new PipelineOfTasks(TaskSource, ExecTaskGenerator);
            ObservingEvents();
        }

        void ObservingEvents()
        {
            PipelineOnBuildStarted = Substitute.For<Action<PipelineStatus>>();
            PipelineOnBuildEnded = Substitute.For<Action<BuildTotalStatus>>();
            PipelineOnBuildUpdated = Substitute.For<Action<PipelineStatus>>();
            PipelineOnTerminalUpdate = Substitute.For<Action<TerminalUpdateInfo>>();
            Pipeline.OnBuildStarted += PipelineOnBuildStarted;
            Pipeline.OnBuildUpdated += PipelineOnBuildUpdated;
            Pipeline.OnBuildEnded += PipelineOnBuildEnded;
            Pipeline.OnTerminalUpdate += PipelineOnTerminalUpdate;
        }
    }
}