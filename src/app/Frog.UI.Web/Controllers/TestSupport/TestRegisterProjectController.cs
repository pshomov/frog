using System.Globalization;
using System.Web.Mvc;
using System.Web;
using Frog.Domain.RepositoryTracker;
using Frog.Support;
using Frog.UI.Web.HttpHelpers;

namespace Frog.UI.Web.Controllers.TestSupport
{
	[HandleError]
    public class TestRegisterProjectController : Controller
    {
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Index(string url)
        {
            if (!url.IsNullOrEmpty() && !url.StartsWith("http://", true, CultureInfo.InvariantCulture))
            {
                ServiceLocator.Bus.Send(new RegisterRepository{Repo = url});
                return MonoBugs.Json(new { projectUrl = Url.Action("status", "TestProject", new { projectUrl = HttpUtility.UrlEncode(PathUrlConversion.Path2Url(url)) }) });
            }
            return new RegisterProjectController().Index(url);
        }
    }
}