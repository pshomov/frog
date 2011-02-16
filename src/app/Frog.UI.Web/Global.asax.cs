using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Frog.Domain;
using Frog.Domain.CustomTasks;
using Frog.Domain.TaskSources;
using Frog.Domain.UI;
using SimpleCQRS;

namespace Frog.UI.Web
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : System.Web.HttpApplication
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }

        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                "Default", // Route name
                "{controller}/{action}/{id}", // URL with parameters
                new { controller = "Home", action = "Index", id = UrlParameter.Optional } // Parameter defaults
            );

        }

        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

            RegisterGlobalFilters(GlobalFilters.Filters);
            RegisterRoutes(RouteTable.Routes);

            var bus = new FakeBus();
            ServiceLocator.Report = new PipelineStatusView.BuildStatus();
            var statusView = new PipelineStatusView(ServiceLocator.Report);
            bus.RegisterHandler<BuildStarted>(statusView.Handle);
            bus.RegisterHandler<BuildEnded>(statusView.Handle);

            var workingAreaPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            var repoArea = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(repoArea);
            Directory.CreateDirectory(workingAreaPath);

            var git_username = Environment.GetEnvironmentVariable("MY_GIT_USERNAME");
            var git_password = Environment.GetEnvironmentVariable("MY_GIT_PASSWORD");

            var driver = new GitDriver(repoArea, "test",
                                       String.Format("https://{0}:{1}@github.com/pshomov/frog.git", git_username,
                                                     git_password));
            var fileFinder = new DefaultFileFinder(new PathFinder());
            var pipeline = new PipelineOfTasks(bus,
                                               new CompoundTaskSource(new MSBuildDetector(fileFinder), new NUnitTaskDetctor(fileFinder)), new ExecTaskGenerator(new ExecTaskFactory()));
            var area = new SubfolderWorkingArea(workingAreaPath);
            ServiceLocator.Valve = new Valve(driver, pipeline, area);
        }
    }
}