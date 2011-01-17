using System.Web.Mvc;

namespace Frog.UI.Web.Controllers
{
    [HandleError]
    public class StatusController : Controller
    {
        public ActionResult Index()
        {
            return Json(new {status = ServiceLocator.Report.Current}, JsonRequestBehavior.AllowGet);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public void Check()
        {
            ServiceLocator.Valve.Check();
        }
    }
}