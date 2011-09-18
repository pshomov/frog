using System.Web.Mvc;
using System.Web.Script.Serialization;
using System.Text;
using Frog.UI.Web;
using Frog.UI.Web.HttpHelpers;

namespace Frog.UI.Web.Controllers
{
    [HandleError]
    public class SystemController : Controller
    {

        public ActionResult Index()
        {
			return MonoBugs.Json(new {status = ServiceLocator.Report});
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult Check()
        {
            return
                Content(
                    "This method is obsolete now, the repository tracker is in its own app and has its own scheduler that is not accessible from the web app");
        }
    }
}