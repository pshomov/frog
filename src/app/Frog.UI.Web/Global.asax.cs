using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Frog.Domain;
using Frog.Domain.Specs;
using Frog.Support;
using SimpleCQRS;

namespace Frog.UI.Web
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : System.Web.HttpApplication
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                "Default", // Route name
                "{controller}/{action}/{id}", // URL with parameters
                new { controller = "Status", action = "Index", id = UrlParameter.Optional } // Parameter defaults
            );

        }

        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

            RegisterRoutes(RouteTable.Routes);

            var bus = new FakeBus();
            ServiceLocator.Report = new BuildStatus();
            var statusView = new PipelineStatusView(ServiceLocator.Report);
            bus.RegisterHandler<BuildStarted>(statusView.Handle);
            bus.RegisterHandler<BuildEnded>(statusView.Handle);

            var workingAreaPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            var repoArea = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(repoArea);
            Directory.CreateDirectory(workingAreaPath);

            string git_username = Environment.GetEnvironmentVariable("MY_GIT_USERNAME");
            string git_password = Environment.GetEnvironmentVariable("MY_GIT_PASSWORD");

            var driver = new GitDriver(repoArea, "test", String.Format("https://{0}:{1}@github.com/pshomov/frog.git", git_username, git_password));
            PipelineOfTasks pipeline;
            if (Underware.IsWindows)
                pipeline = new PipelineOfTasks(bus, new ExecTask(@"cmd.exe", @"/c %SystemRoot%\Microsoft.NET\Framework\v3.5\msbuild.exe Frog.Net.sln"));
            else
                pipeline = new PipelineOfTasks(bus, new ExecTask(@"xbuild", @"Frog.Net.sln"));
            var area = new SubfolderWorkingArea(workingAreaPath);
            ServiceLocator.Valve = new Valve(driver, pipeline, area);
        }

        public class PipelineStatusView : Handles<BuildStarted>, Handles<BuildEnded>
        {
            readonly BuildStatus report;

            public PipelineStatusView(BuildStatus report)
            {
                this.report = report;
            }

            public void Handle(BuildStarted message)
            {
                report.Current = BuildStatus.Status.Started;
            }

            public void Handle(BuildEnded message)
            {
                report.Current = BuildStatus.Status.Complete;
            }
        }

        public class BuildStatus
        {
            public enum Status { Started, NotStarted, Complete }
            public Status Current { get; set; }
        }
    }
}