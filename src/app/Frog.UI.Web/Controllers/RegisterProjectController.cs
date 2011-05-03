using System.Web.Mvc;
using Frog.Domain;
using Frog.Support;
using Frog.UI.Web.Controllers.TestSupport;

namespace Frog.UI.Web.Controllers
{
    [HandleError]
    public class RegisterProjectController : Controller
    {
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Index(string url)
        {
            if (url.IsNullOrEmpty())
            {
                return MonoBugs.Json(new {error = "Url was not provided"});
            }
            ServiceLocator.Bus.Send(new RegisterRepository { Repo = url });
            return MonoBugs.Json(new { projectUrl = Url.Action("status", "Project", new { user = "u", project = "p" }) });
        }
    }
}