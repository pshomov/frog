using System.Web;
using System.Web.Mvc;
using Frog.Support;

namespace Frog.UI.Web.Controllers
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
    }
}