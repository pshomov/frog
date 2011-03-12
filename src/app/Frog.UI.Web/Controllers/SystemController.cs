using System.Web.Mvc;
using System.Web.Script.Serialization;
using System.Text;
using Frog.UI.Web;

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
        public void Check()
        {
            ServiceLocator.RepositoryTracker.CheckForUpdates();
        }
    }
}