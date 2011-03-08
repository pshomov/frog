using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Frog.UI.Web.Controllers
{
    public class RegisterProjectController : Controller
    {
        //
        // GET: /RegisterProject/

        [AcceptVerbs("POST", "GET")]
        public ActionResult Index(string url)
        {
            ServiceLocator.RepositoryTracker.Track(url);
            return Redirect("~/Content/status.html");
        }

    }
}
