using System.Web;
using System.Web.Mvc;
using Frog.Support;

namespace Frog.UI.Web.Controllers.TestSupport
{
    [HandleError]
    public class TestProjectController : Controller
    {
        public ActionResult Status(string projectUrl)
        {
            return View();
        }

        public ActionResult Data(string projectUrl)
        {
            if (Os.IsWindows)
                projectUrl = HttpUtility.UrlDecode(projectUrl);
            return new ProjectController().GetProjectStatus(PathUrlConversion.Url2Path(projectUrl));
        }

        public ActionResult TerminalOutput(string projectUrl, int taskIndex)
        {
            if (Os.IsWindows)
                projectUrl = HttpUtility.UrlDecode(projectUrl);
            return new ProjectController().GetTaskTerminalOutput(PathUrlConversion.Url2Path(projectUrl), taskIndex);
        }

        public ActionResult AllTerminalOutput(string projectUrl, int lastOutputChunkIndex)
        {
            if (Os.IsWindows)
                projectUrl = HttpUtility.UrlDecode(projectUrl);
            return ProjectController.GetAllTaskTerminalOutput(PathUrlConversion.Url2Path(projectUrl), lastOutputChunkIndex);            
        }
    }
}