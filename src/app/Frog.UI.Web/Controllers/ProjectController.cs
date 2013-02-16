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
    [HandleError]
    public class ProjectController : Controller
    {
        public ActionResult AllTerminalOutput(string user, string project, int lastOutputChunkIndex, int taskIndex)
        {
            return GetAllTaskTerminalOutput(GetGithubProjectUrl(user, project), lastOutputChunkIndex, taskIndex);
        }

        public ActionResult Data(string user, string project)
        {
            return GetProjectStatus(GetGithubProjectUrl(user, project));
        }

        public ActionResult Force(string user, string project)
        {
            string githubProjectUrl = GetGithubProjectUrl(user, project);
            ProjectBuild lastBuild =
                ServiceLocator.Store.GetReader<ProjectId, ProjectHistory>()
                              .Load(new ProjectId(githubProjectUrl))
                              .CurrentHistory;
            ServiceLocator.Bus.Send(new Build
                {
                    Id = Guid.NewGuid(),
                    RepoUrl = githubProjectUrl,
                    Revision = new RevisionInfo {Revision = lastBuild.RevisionNr}
                });
            return RedirectToAction("Status");
        }

        public ActionResult GetProjectHistory(string projectUrl)
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

        public ActionResult History(string user, string project)
        {
            return GetProjectHistory(GetGithubProjectUrl(user, project));
        }

        public ActionResult Status(string user, string project)
        {
            return View();
        }

        public ActionResult TerminalOutput(string user, string project, int taskIndex)
        {
            return GetTaskTerminalOutput(GetGithubProjectUrl(user, project), taskIndex);
        }

        internal static ActionResult GetAllTaskTerminalOutput(string projectUrl, int lastChunkIndex, int taskIndex)
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
                    content.Append(terminalOutputR);
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

        string GetGithubProjectUrl(string user, string project)
        {
            return String.Format("http://github.com/{0}/{1}", user, project);
        }


        protected internal ActionResult GetProjectStatus(string projectUrl)
        {
            IDocumentReader<ProjectId, ProjectHistory> documentReader =
                ServiceLocator.Store.GetReader<ProjectId, ProjectHistory>();
            ProjectHistory projectHistory;
            if (documentReader.TryGet(new ProjectId(projectUrl), out projectHistory))
            {
                return MonoBugs.Json(new {status = projectHistory.Current});
            }
            return new HttpNotFoundResult("Project does not Runz ;(");
        }

        protected internal ActionResult GetTaskTerminalOutput(string projectUrl, int taskIndex)
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