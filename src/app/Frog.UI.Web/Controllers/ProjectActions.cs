using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Frog.Domain;
using Frog.UI.Web.HttpHelpers;
using Lokad.Cqrs.AtomicStorage;
using SaaS.Client.Projections.Frog.Projects;
using SaaS.Engine;
using Build = Frog.Domain.RepositoryTracker.Build;
using TaskInfo = SaaS.Engine.TaskInfo;

namespace Frog.UI.Web.Controllers
{
    public class ProjectActions
    {
        public static void ForceBuild(string projectUrl)
        {
            ProjectBuild lastBuild =
                ServiceLocator.Store.GetReader<ProjectId, ProjectHistory>()
                              .Load(new ProjectId(projectUrl))
                              .CurrentHistory;
            ServiceLocator.Bus.Send(new Build
                {
                    Id = Guid.NewGuid(),
                    RepoUrl = lastBuild.ProjectUrl,
                    Revision = new RevisionInfo {Revision = lastBuild.RevisionNr}
                });
        }

        public static ActionResult GetProjectHistory(string projectUrl)
        {
            IDocumentReader<ProjectId, ProjectHistory> documentReader =
                ServiceLocator.Store.GetReader<ProjectId, ProjectHistory>();
            ProjectHistory projectHistory;
            if (documentReader.TryGet(new ProjectId(projectUrl), out projectHistory))
            {
                return MonoBugs.Json(
                    new
                        {
                            items =
                        projectHistory.Items
                        });
            }
            return new HttpNotFoundResult("Project does not Runz ;(");
        }

        public static ActionResult GetAllTaskTerminalOutput(string projectUrl, int lastChunkIndex, int taskIndex)
        {
            IDocumentReader<ProjectId, ProjectHistory> documentReader =
                ServiceLocator.Store.GetReader<ProjectId, ProjectHistory>();
            ProjectHistory projectHistory;
            if (documentReader.TryGet(new ProjectId(projectUrl), out projectHistory))
            {
                List<TaskInfo> tasks = projectHistory.Current.Tasks;
                int activeTask = taskIndex;
                var content = new StringBuilder();
                for (int i = taskIndex; i < tasks.Count; i++)
                {
                    int sinceIndex = i == taskIndex ? lastChunkIndex : 0;
                    TerminalId terminalId = projectHistory.Current.Tasks[i].Id;
                    List<string> terminalOutput = projectHistory.Current.TerminalOutput[terminalId];
                    IEnumerable<string> terminalOutputR =
                        terminalOutput.Where((s, i1) => i1 > sinceIndex);
                    if (terminalOutput.Count <= sinceIndex) continue;
                    activeTask = i;
                    lastChunkIndex = terminalOutput.Count;
                    content.Append(terminalOutputR.DefaultIfEmpty().Aggregate((s, s1) => s + s1 + "\r\n"));
                }
                return
                    MonoBugs.Json(
                        new
                            {
                                terminalOutput = content.ToString(),
                                activeTask,
                                lastChunkIndex
                            });
            }
            return new HttpNotFoundResult("Project does not Runz ;(");
        }

        public static string GetGithubProjectUrl(string user, string project)
        {
            return String.Format("http://github.com/{0}/{1}", user, project);
        }

        public static string GetCodebaseProjectUrl(string company, string project, string repository)
        {
            return String.Format("http://{0}.codebasehq.com/{1}/{2}.git", company, project, repository);
        }

        public static ActionResult GetProjectStatus(string projectUrl)
        {
            IDocumentReader<ProjectId, ProjectHistory> documentReader =
                ServiceLocator.Store.GetReader<ProjectId, ProjectHistory>();
            ProjectHistory projectHistory;
            if (documentReader.TryGet(new ProjectId(projectUrl), out projectHistory))
            {
                return MonoBugs.Json(new {status = projectHistory.Current.Status, tasks = projectHistory.CurrentTasks});
            }
            return new HttpNotFoundResult("Project does not Runz ;(");
        }

        public static ActionResult GetTaskTerminalOutput(string projectUrl, int taskIndex)
        {
            IDocumentReader<ProjectId, ProjectHistory> documentReader =
                ServiceLocator.Store.GetReader<ProjectId, ProjectHistory>();
            ProjectHistory projectHistory;
            if (documentReader.TryGet(new ProjectId(projectUrl), out projectHistory))
            {
                TaskInfo taskInfo = projectHistory.Current.Tasks[taskIndex];
                MonoBugs.Json(
                    new
                        {
                            terminalOutput = projectHistory.Current.TerminalOutput[taskInfo.Id]
                        });
            }
            return new HttpNotFoundResult("Project does not Runz ;(");
        }
    }
}