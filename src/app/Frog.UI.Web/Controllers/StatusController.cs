using System.Web.Mvc;

namespace Frog.UI.Web.Controllers
{
    [HandleError]
    public class StatusController : Controller
    {
        public ActionResult Index()
        {
            return Json(new {name = "Petar"}, JsonRequestBehavior.AllowGet);
        }
    }
}