using System;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Frog.Domain;
using Frog.Domain.RepositoryTracker;
using Frog.UI.Web.HttpHelpers;
using Lokad.Cqrs;
using SaaS.Client.Projections.Frog.Projects;
using SaaS.Client.Projections.Releases;
using Lokad.Cqrs.AtomicStorage;
using SaaS.Engine;

namespace Frog.UI.Web.Controllers
{
    [HandleError]
    public class ProjectController : Controller
    {
        public ActionResult Status(string user, string project)
        {
            return View();
        }

        public ActionResult Force(string user, string project)
        {
            var githubProjectUrl = GetGithubProjectUrl(user, project);
            var lastBuild = ServiceLocator.Store.GetReader<ProjectId, ProjectHistory>().Load(new ProjectId(githubProjectUrl)).CurrentHistory;
            ServiceLocator.Bus.Send(new Frog.Domain.RepositoryTracker.Build(){Id = Guid.NewGuid(), RepoUrl = githubProjectUrl, Revision = new RevisionInfo(){Revision = lastBuild.RevisionNr}});
            return RedirectToAction("Status");
        }

        public ActionResult History(string user, string project)
        {
            return GetProjectHistory(GetGithubProjectUrl(user, project));
        }

        public ActionResult Data(string user, string project)
        {
            return GetProjectStatus(GetGithubProjectUrl(user, project));
        }

        public ActionResult TerminalOutput(string user, string project, int taskIndex)
        {
            return GetTaskTerminalOutput(GetGithubProjectUrl(user, project), taskIndex);
        }

        public ActionResult AllTerminalOutput(string user, string project, int lastOutputChunkIndex, int taskIndex)
        {
            return GetAllTaskTerminalOutput(GetGithubProjectUrl(user, project), lastOutputChunkIndex, taskIndex);
        }

        internal static ActionResult GetAllTaskTerminalOutput(string projectUrl, int lastChunkIndex, int taskIndex)
        {
            var documentReader = ServiceLocator.Store.GetReader<ProjectId, ProjectHistory>();
            ProjectHistory projectHistory;
            if (documentReader.TryGet(new ProjectId(projectUrl), out projectHistory))
            {
                var tasks = projectHistory.Current.Tasks;
                var activeTask = taskIndex;
                var content = new StringBuilder();
                for (var i = taskIndex; i < tasks.Count; i++)
                {
                    var sinceIndex = i == taskIndex ? lastChunkIndex : 0;
                    var terminalId = projectHistory.Current.Tasks[i].Id;
                    var terminalOutput = projectHistory.Current.TerminalOutput[terminalId];
                    var terminalOutputR =
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
                                terminalOutput = content.ToString(), activeTask, lastChunkIndex
                            });
                            
            }

//            if (ServiceLocator.ProjectStatus.IsProjectRegistered(projectUrl))
//            {
//                var tasks = ServiceLocator.BuildStatus.GetBuildStatus(ServiceLocator.ProjectStatus.GetCurrentBuild(projectUrl)).Tasks;
//                var activeTask = taskIndex;
//                var content = new StringBuilder();
//                for (var i = taskIndex; i < tasks.Count; i++)
//                {
//                    var sinceIndex = i == taskIndex ? lastChunkIndex : 0;
//                    var terminalOutput = ServiceLocator.TerminalOutputStatus.GetTerminalOutput(tasks[i].TerminalId, sinceIndex);
//                    if (terminalOutput.LastChunkIndex <= sinceIndex) continue;
//                    activeTask = i;
//                    lastChunkIndex = terminalOutput.LastChunkIndex;
//                    content.Append(terminalOutput.Content);
//                }
//                return
//                    MonoBugs.Json(
//                        new
//                            {
//                                terminalOutput = content.ToString(), activeTask, lastChunkIndex
//                            });
//                
//            }
            return new HttpNotFoundResult("Project does not Runz ;(");
        }

        string GetGithubProjectUrl(string user, string project)
        {
            return String.Format("http://github.com/{0}/{1}", user, project);
        }


        protected internal ActionResult GetProjectStatus(string projectUrl)
        {
            var documentReader = ServiceLocator.Store.GetReader<ProjectId, ProjectHistory>();
            ProjectHistory projectHistory;
            if (documentReader.TryGet(new ProjectId(projectUrl), out projectHistory))
            {
               return MonoBugs.Json(new { status = projectHistory.Current.Status });
            }
//            if (ServiceLocator.ProjectStatus.IsProjectRegistered(projectUrl))
//                return MonoBugs.Json(new { status = ServiceLocator.BuildStatus.GetBuildStatus(ServiceLocator.ProjectStatus.GetCurrentBuild(projectUrl)) });
            return new HttpNotFoundResult("Project does not Runz ;(");
        }

        public ActionResult GetProjectHistory(string projectUrl)
        {
            var documentReader = ServiceLocator.Store.GetReader<ProjectId, ProjectHistory>();
            ProjectHistory projectHistory;
            if (documentReader.TryGet(new ProjectId(projectUrl), out projectHistory))
            {
                    MonoBugs.Json(
                        new
                        {
                            items =
                        projectHistory.Items
                        });
            }
//         if (ServiceLocator.ProjectStatus.IsProjectRegistered(projectUrl))
//                return
//                    MonoBugs.Json(
//                        new
//                        {
//                            items =
//                        ServiceLocator.ProjectStatus.GetListOfBuilds(projectUrl)
//                        });
            return new HttpNotFoundResult("Project does not Runz ;(");
        }

        protected internal ActionResult GetTaskTerminalOutput(string projectUrl, int taskIndex)
        {
            var documentReader = ServiceLocator.Store.GetReader<ProjectId, ProjectHistory>();
            ProjectHistory projectHistory;
            if (documentReader.TryGet(new ProjectId(projectUrl), out projectHistory))
            {
                var taskInfo = projectHistory.Current.Tasks[taskIndex];
                MonoBugs.Json(
                    new
                        {
                            terminalOutput = projectHistory.Current.TerminalOutput[taskInfo.Id]
                        });
            }
//            if (ServiceLocator.ProjectStatus.IsProjectRegistered(projectUrl))
//                return
//                    MonoBugs.Json(
//                        new
//                            {
//                                terminalOutput =
//                            ServiceLocator.TerminalOutputStatus.GetTerminalOutput(ServiceLocator.BuildStatus.GetBuildStatus(ServiceLocator.ProjectStatus.GetCurrentBuild(projectUrl)).Tasks[taskIndex].TerminalId)
//                            });
            return new HttpNotFoundResult("Project does not Runz ;(");
        }
    }
}