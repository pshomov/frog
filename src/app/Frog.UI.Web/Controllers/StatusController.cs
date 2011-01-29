using System.Web.Mvc;

namespace Frog.UI.Web.Controllers
{
    [HandleError]
    public class StatusController : Controller
    {
        public ActionResult Index()
        {
            return Json(new {status = ServiceLocator.Report.Current, name = "Frog3"}, JsonRequestBehavior.AllowGet);
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public void Check()
        {
            ServiceLocator.Valve.Check();
        }
    }
}