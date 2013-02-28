using System.Web.Mvc;

namespace Frog.UI.Web.Controllers
{
    [HandleError]
    public class ProjectController : Controller
    {

        public ActionResult AllTerminalOutput(string user, string project, int lastOutputChunkIndex, int taskIndex)
        {
            return Controllers.ProjectActions.GetAllTaskTerminalOutput(ProjectActions.GetGithubProjectUrl(user, project), lastOutputChunkIndex, taskIndex);
        }

        public ActionResult Data(string user, string project)
        {
            return ProjectActions.GetProjectStatus(ProjectActions.GetGithubProjectUrl(user, project));
        }

        public ActionResult Force(string user, string project)
        {
            string githubProjectUrl = ProjectActions.GetGithubProjectUrl(user, project);
            ProjectActions.ForceBuild(githubProjectUrl);
            return RedirectToAction("Status");
        }

        public ActionResult History(string user, string project)
        {
            return ProjectActions.GetProjectHistory(ProjectActions.GetGithubProjectUrl(user, project));
        }

        public ActionResult Status(string user, string project)
        {
            return View();
        }

        public ActionResult TerminalOutput(string user, string project, int taskIndex)
        {
            return ProjectActions.GetTaskTerminalOutput(ProjectActions.GetGithubProjectUrl(user, project), taskIndex);
        }
    }
}