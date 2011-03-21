using System.Globalization;
using System.Web.Mvc;
using System.Web;
using Frog.Support;

namespace Frog.UI.Web.Controllers
{
	[HandleError]
    public class TestRegisterProjectController : Controller
    {
        [AcceptVerbs(HttpVerbs.Post)]
        public new ActionResult Index(string url)
        {
            if (!url.IsNullOrEmpty() && !url.StartsWith("http://", true, CultureInfo.InvariantCulture))
            {
                ServiceLocator.RepositoryTracker.Track(url);
                return MonoBugs.Json(new { projectUrl = Url.Action("status", "TestProject", new { projectUrl = HttpUtility.UrlEncode(PathUrlConversion.Path2Url(url)) }) });
            }
            return new RegisterProjectController().Index(url);
        }
    }
}