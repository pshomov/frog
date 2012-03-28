using System;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Frog.UI.Web.HttpHelpers;

namespace Frog.UI.Web.Controllers
{
    [HandleError]
    public class ProjectController : Controller
    {
        public ActionResult Status(string user, string project)
        {
            return View();
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
            if (ServiceLocator.Report.IsProjectRegistered(projectUrl))
            {
                var tasks = ServiceLocator.Report.GetBuildStatus(ServiceLocator.Report.GetCurrentBuild(projectUrl)).Tasks;
                var activeTask = taskIndex;
                var content = new StringBuilder();
                for (var i = taskIndex; i < tasks.Count; i++)
                {
                    var sinceIndex = i == taskIndex ? lastChunkIndex : 0;
                    var terminalOutput = tasks[i].GetTerminalOutput(sinceIndex);
                    if (terminalOutput.LastChunkIndex <= sinceIndex) continue;
                    activeTask = i;
                    lastChunkIndex = terminalOutput.LastChunkIndex;
                    content.Append(terminalOutput.Content);
                }
                return
                    MonoBugs.Json(
                        new
                            {
                                terminalOutput = content.ToString(), activeTask, lastChunkIndex
                            });
                
            }
            else
            {
                return new HttpNotFoundResult("Project does not Runz ;(");
            }
        }

        string GetGithubProjectUrl(string user, string project)
        {
            return String.Format("http://github.com/{0}/{1}.git", user, project);
        }


        protected internal ActionResult GetProjectStatus(string projectUrl)
        {
            if (ServiceLocator.Report.IsProjectRegistered(projectUrl))
                return MonoBugs.Json(new { status = ServiceLocator.Report.GetBuildStatus(ServiceLocator.Report.GetCurrentBuild(projectUrl)) });
            else
            {
                return new HttpNotFoundResult("Project does not Runz ;(");
            }
        }

        public ActionResult GetProjectHistory(string projectUrl)
        {
            if (ServiceLocator.Report.IsProjectRegistered(projectUrl))
                return
                    MonoBugs.Json(
                        new
                        {
                            items =
                        ServiceLocator.Report.GetListOfBuilds(projectUrl)
                        });
            else
            {
                return new HttpNotFoundResult("Project does not Runz ;(");
            }
        }

        protected internal ActionResult GetTaskTerminalOutput(string projectUrl, int taskIndex)
        {
            if (ServiceLocator.Report.IsProjectRegistered(projectUrl))
                return
                    MonoBugs.Json(
                        new
                            {
                                terminalOutput =
                            ServiceLocator.Report.GetBuildStatus(ServiceLocator.Report.GetCurrentBuild(projectUrl)).Tasks[taskIndex].GetTerminalOutput()
                            });
            else
            {
                return new HttpNotFoundResult("Project does not Runz ;(");
            }
        }
    }
}