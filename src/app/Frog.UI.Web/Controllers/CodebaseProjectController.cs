using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;

namespace Frog.UI.Web.Controllers
{
    public class CodebaseProjectController : Controller
    {
        public ActionResult AllTerminalOutput(string company, string project, string repository, int lastOutputChunkIndex, int taskIndex)
        {
            return ProjectActions.GetAllTaskTerminalOutput(ProjectActions.GetCodebaseProjectUrl(company, project, repository), lastOutputChunkIndex, taskIndex);
        }

        public ActionResult Data(string company, string project, string repository)
        {
            return ProjectActions.GetProjectStatus(ProjectActions.GetCodebaseProjectUrl(company, project, repository));
        }

        public ActionResult Force(string company, string project, string repository)
        {
            ProjectActions.ForceBuild(ProjectActions.GetCodebaseProjectUrl(company, project, repository));
            return RedirectToAction("Status");
        }

        public ActionResult History(string company, string project, string repository)
        {
            return ProjectActions.GetProjectHistory(ProjectActions.GetCodebaseProjectUrl(company, project, repository));
        }

        public ActionResult Status(string user, string project)
        {
            return View();
        }

        public ActionResult TerminalOutput(string company, string project, string repository, int taskIndex)
        {
            return ProjectActions.GetTaskTerminalOutput(ProjectActions.GetCodebaseProjectUrl(company, project, repository), taskIndex);
        }


//        public ActionResult _Genereic_Index(string segments)
//        {
//            var match = new Regex(@"(\w+\/)(\w+\/)?(\w+\/)?(\w+)?").Match(segments);
//            if (match.Success)
//            {
//                var pieces = new List<string>();
//                for (int i = 1; i < match.Groups.Count; i++)
//                {
//                    pieces.Add(match.Groups[i].Value.TrimEnd('/'));
//                }
//                if (pieces.Last() == "status")
//                {
//                    var path = pieces.Take(pieces.Count - 1).Where(s => s != "");
//                    var projectUrl = GetProjectUrl(path);
//                }
//            }
//            return View();
//        }

        private string GetProjectUrl(IEnumerable<string> path)
        {
            var result = "http:/";
            result = path.Aggregate(result, (s, s1) => s + "/" + s1);
            return result;
        }
    }
}
