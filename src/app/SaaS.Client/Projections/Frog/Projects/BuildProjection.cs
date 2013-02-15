using System.Collections.Generic;
using System.Diagnostics.Contracts;
using SaaS.Client.Projections.Frog.Projects;
using SaaS.Engine;

static internal class BuildProjection
{
    public static void OnBuildStarted(BuildStarted e, Build view)
    {
        view.Tasks = new List<TaskInfo>(e.Status.Tasks);
        view.Tasks.ForEach(info => view.TerminalOutput[info.Id] = new List<string>(){""});
        view.Status = Build.BuildOverallStatus.Started;
    }

    public static void OnBuildUpdated(BuildUpdated e, Build view)
    {
        var existing = view.Tasks[e.TaskIndex];
        view.Tasks[e.TaskIndex] = new TaskInfo(existing.Id, existing.Name, e.TaskStatus);
    }

    public static void OnTerminalOutput(TerminalUpdated e, Build view)
    {
        view.TerminalOutput[e.Id].Add(e.Content);
    }

    public static Build.BuildOverallStatus OnBuildEnded(BuildEnded e, Build view)
    {
        return view.Status = e.Status == BuildTotalEndStatus.Error ? Build.BuildOverallStatus.EndedFailure : Build.BuildOverallStatus.EndedSuccess;
    }
}