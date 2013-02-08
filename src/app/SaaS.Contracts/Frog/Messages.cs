using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

// ReSharper disable PartialTypeWithSinglePart
// ReSharper disable UnusedMember.Local
namespace SaaS.Engine
{
    #region Generated by Lokad Code DSL
    [DataContract(Namespace = "Frog.Stuff")]
public partial class TaskInfo
    {
        [DataMember(Order = 1)] public TerminalId Id { get; private set; }
        [DataMember(Order = 2)] public string Name { get; private set; }
        [DataMember(Order = 3)] public TaskStatus Status { get; private set; }
        
        TaskInfo () {}
        public TaskInfo (TerminalId id, string name, TaskStatus status)
        {
            Id = id;
            Name = name;
            Status = status;
        }
    }
    [DataContract(Namespace = "Frog.Stuff")]
public partial class PipelineStatus
    {
        [DataMember(Order = 1)] public TaskInfo[] Tasks { get; private set; }
        
        PipelineStatus () 
{
            Tasks = new TaskInfo[0];
        }
        public PipelineStatus (TaskInfo[] tasks)
        {
            Tasks = tasks;
        }
    }
    [DataContract(Namespace = "Frog.Stuff")]
public partial class BuildStarted : IEvent<BuildId>
    {
        [DataMember(Order = 1)] public BuildId Id { get; private set; }
        [DataMember(Order = 2)] public string RepoUrl { get; private set; }
        [DataMember(Order = 3)] public PipelineStatus Status { get; private set; }
        
        BuildStarted () {}
        public BuildStarted (BuildId id, string repoUrl, PipelineStatus status)
        {
            Id = id;
            RepoUrl = repoUrl;
            Status = status;
        }
        
        public override string ToString()
        {
            return string.Format(@"It is done");
        }
    }
    [DataContract(Namespace = "Frog.Stuff")]
public partial class BuildEnded : IEvent<BuildId>
    {
        [DataMember(Order = 1)] public BuildId Id { get; private set; }
        [DataMember(Order = 2)] public BuildTotalEndStatus Status { get; private set; }
        
        BuildEnded () {}
        public BuildEnded (BuildId id, BuildTotalEndStatus status)
        {
            Id = id;
            Status = status;
        }
    }
    [DataContract(Namespace = "Frog.Stuff")]
public partial class BuildUpdated : IEvent<BuildId>
    {
        [DataMember(Order = 1)] public BuildId Id { get; private set; }
        [DataMember(Order = 2)] public int TaskIndex { get; private set; }
        [DataMember(Order = 3)] public TaskStatus TaskStatus { get; private set; }
        
        BuildUpdated () {}
        public BuildUpdated (BuildId id, int taskIndex, TaskStatus taskStatus)
        {
            Id = id;
            TaskIndex = taskIndex;
            TaskStatus = taskStatus;
        }
    }
    [DataContract(Namespace = "Frog.Stuff")]
public partial class TerminalUpdated : IEvent<TerminalId>
    {
        [DataMember(Order = 1)] public TerminalId Id { get; private set; }
        [DataMember(Order = 2)] public string Content { get; private set; }
        [DataMember(Order = 3)] public int ContentSequnceIndex { get; private set; }
        [DataMember(Order = 4)] public BuildId BuildId { get; private set; }
        
        TerminalUpdated () {}
        public TerminalUpdated (TerminalId id, string content, int contentSequnceIndex, BuildId buildId)
        {
            Id = id;
            Content = content;
            ContentSequnceIndex = contentSequnceIndex;
            BuildId = buildId;
        }
    }
    [DataContract(Namespace = "Frog.Stuff")]
public partial class ProjectRegistered : IEvent<ProjectId>
    {
        [DataMember(Order = 1)] public ProjectId Id { get; private set; }
        [DataMember(Order = 2)] public string RepoUrl { get; private set; }
        
        ProjectRegistered () {}
        public ProjectRegistered (ProjectId id, string repoUrl)
        {
            Id = id;
            RepoUrl = repoUrl;
        }
    }
    
    public interface IProjectState
    {
        void When(ProjectRegistered e);
    }
    
    public interface ITaskState
    {
        void When(TerminalUpdated e);
    }
    
    public interface IBuildState
    {
        void When(BuildStarted e);
        void When(BuildEnded e);
        void When(BuildUpdated e);
    }
    #endregion
}