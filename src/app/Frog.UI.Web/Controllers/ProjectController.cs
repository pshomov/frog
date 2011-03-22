using System;
using System.Web.Mvc;
using Frog.UI.Web.HttpHelpers;

namespace Frog.UI.Web.Controllers
{
    [HandleError]
    public class ProjectController : Controller
    {
        public ActionResult Status(string user, string project)
        {
            return View();
        }

        public ActionResult Data(string user, string project)
        {
            string projectUrl = String.Format("http://github.com/{0}/{1}.git", user, project);
            return GetProjectStatus(projectUrl);
        }


        protected internal ActionResult GetProjectStatus(string projectUrl)
        {
            if (ServiceLocator.Report.ContainsKey(projectUrl))
                return MonoBugs.Json(new {status = ServiceLocator.Report[projectUrl]});
            else
            {
                return new HttpNotFoundResult("Project does not Runz ;(");
            }
        }
    }
}