﻿using System;
using Frog.Specs.Support;
using NSubstitute;

namespace Frog.Domain.Specs
{
    public abstract class PipelineProcessSpecBase : BDD
    {
        protected Domain.Pipeline Pipeline;
        protected ExecutableTask Task1;
        protected ExecutableTask Task2;
        protected TaskSource TaskSource;
        protected ExecTaskGenerator ExecTaskGenerator;
        protected Pipeline.BuildStartedDelegate PipelineOnBuildStarted;
        protected Action<BuildTotalEndStatus> PipelineOnBuildEnded;
        protected Action<int, Guid, TaskInfo.TaskStatus> PipelineOnBuildUpdated;
        protected Action<TerminalUpdateInfo> PipelineOnTerminalUpdate;
        protected int counter = 0;

        protected override void Given()
        {
            TaskSource = Substitute.For<TaskSource>();
            ExecTaskGenerator = Substitute.For<ExecTaskGenerator>();
            Pipeline = new Pipeline(TaskSource, ExecTaskGenerator);
            ObservingEvents();
        }

        void ObservingEvents()
        {
            PipelineOnBuildStarted = Substitute.For<Pipeline.BuildStartedDelegate>();
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