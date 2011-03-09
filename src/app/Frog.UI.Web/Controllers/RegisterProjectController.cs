using System.Web.Mvc;
using Frog.Support;

namespace Frog.UI.Web.Controllers
{
    public class RegisterProjectController : Controller
    {
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Index(string url)
        {
            if (url.IsNullOrEmpty())
            {
                return MonoBugs.Json(new {error = "Url was not provided"});
            }
            ServiceLocator.RepositoryTracker.Track(url);
            return Redirect("~/Content/status.html");
        }
    }
}