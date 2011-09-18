using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Frog.UI.Web.HttpHelpers;

namespace Frog.UI.Web.Controllers
{
    public class DiagnosticsController : Controller
    {
        public ActionResult AllMessages()
        {
            return MonoBugs.Json(ServiceLocator.AllMassages.Select(message => new {messageType = message.GetType().Name, fields = message}));
        }        
    }
}
