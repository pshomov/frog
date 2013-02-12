using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Frog.UI.Web.HttpHelpers;
using Lokad.Cqrs;
using SaaS.Client.Projections.Frog.Projects;
using SaaS.Client.Projections.Releases;

namespace Frog.UI.Web.Controllers
{
    public class AllProjectsController : Controller
    {
        //
        // GET: /AllProjects/

        public ActionResult Index()
        {
            var store = ServiceLocator.Store;
            var documentReader = store.GetReader<unit, Projects>();
            Projects p = null;
            if (documentReader.TryGet(unit.it, out p))
            {
                return MonoBugs.Json(new {p = p});
            }
            else
            {
                return MonoBugs.Json(new {msg = "Nothing foun"});
            }

        }

    }
}
