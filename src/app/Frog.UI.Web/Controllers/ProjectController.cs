using System;
using System.Linq;
using System.Web.Mvc;

namespace Frog.UI.Web.Controllers
{
    [HandleError]
    public class ProjectController : Controller
    {
        public ActionResult Status(string user, string project)
        {
            return View();
        }

        public ActionResult Data(string user, string project)
        {
            return GetProjectStatus(GetGithubProjectUrl(user, project));
        }

        public ActionResult TerminalOutput(string user, string project, int taskIndex)
        {
            return GetTaskTerminalOutput(GetGithubProjectUrl(user, project), taskIndex);
        }

        public ActionResult AllTerminalOutput(string user, string project, int lastOuputChunkIndex)
        {
            return GetAllTaskTerminalOutput(GetGithubProjectUrl(user, project), lastOuputChunkIndex);
        }

        internal static ActionResult GetAllTaskTerminalOutput(string projectUrl, int lastKnownIndex)
        {
            if (ServiceLocator.Report.ContainsKey(projectUrl))
                return
                    MonoBugs.Json(
                        new
                        {
                            terminalOutput =
                        ServiceLocator.Report[projectUrl].Tasks.Select(state => state.TerminalOutput).Aggregate((s, s1) => s+s1)
                        });
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
            if (ServiceLocator.Report.ContainsKey(projectUrl))
                return MonoBugs.Json(new {status = ServiceLocator.Report[projectUrl]});
            else
            {
                return new HttpNotFoundResult("Project does not Runz ;(");
            }
        }

        protected internal ActionResult GetTaskTerminalOutput(string projectUrl, int taskIndex)
        {
            if (ServiceLocator.Report.ContainsKey(projectUrl))
                return
                    MonoBugs.Json(
                        new
                            {
                                terminalOutput =
                            ServiceLocator.Report[projectUrl].Tasks[taskIndex].TerminalOutput
                            });
            else
            {
                return new HttpNotFoundResult("Project does not Runz ;(");
            }
        }
    }
}