﻿using System;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace Frog.UI.Web.Controllers
{
    public class ProjectController : Controller
    {
        public ActionResult Status(string user, string project)
        {
            return View();
        }

        public ActionResult Data(string user, string project)
        {
            string projectUrl = String.Format("http://github.com/{0}/{1}.git", user, project);
            if (ServiceLocator.Report.ContainsKey(projectUrl))
                return MonoBugs.Json(new {status = ServiceLocator.Report[projectUrl]});
            else
            {
                return new HttpNotFoundResult("Project does not Runz ;(");
            }
        }
    }

    public class HttpNotFoundResult : ActionResult
    {
        /// <summary>
        ///   Initializes a new instance of <see cref = "HttpNotFoundResult" /> with the specified <paramref name = "message" />.
        /// </summary>
        /// <param name = "message"></param>
        public HttpNotFoundResult(String message)
        {
            this.Message = message;
        }

        /// <summary>
        ///   Initializes a new instance of <see cref = "HttpNotFoundResult" /> with an empty message.
        /// </summary>
        public HttpNotFoundResult()
            : this(String.Empty)
        {
        }

        /// <summary>
        ///   Gets or sets the message that will be passed to the thrown <see cref = "HttpException" />.
        /// </summary>
        public String Message { get; set; }

        /// <summary>
        ///   Overrides the base <see cref = "ActionResult.ExecuteResult" /> functionality to throw an <see cref = "HttpException" />.
        /// </summary>
        public override void ExecuteResult(ControllerContext context)
        {
            throw new HttpException((Int32) HttpStatusCode.NotFound, this.Message);
        }
    }
}