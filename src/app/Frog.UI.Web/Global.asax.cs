using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
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

    public class MvcApplication : HttpApplication
    {
        string mapPath;

        static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute("status", "project/github/{user}/{project}/{action}", new {controller = "Project", action = "status"});
            routes.MapRoute(
                "Default", // Route name
                "{controller}/{action}/{id}", // URL with parameters
                new {controller = "Home", action = "Index", id = UrlParameter.Optional} // Parameter defaults
                );
        }

        protected void Application_Start()
        {
            mapPath = Server.MapPath("~/App_Data/Crash_");
            AppDomain.CurrentDomain.UnhandledException += CurrentDomainOnUnhandledException;
            AreaRegistration.RegisterAllAreas();

            RegisterRoutes(RouteTable.Routes);

            SetupApp();
        }

        void CurrentDomainOnUnhandledException(object sender, UnhandledExceptionEventArgs unhandledExceptionEventArgs)
        {
            try
            {
                Exception ex = (Exception) unhandledExceptionEventArgs.ExceptionObject;
                Console.WriteLine("Exception is: \n" + ex);
                using (var crashFile = new StreamWriter(mapPath + DateTime.UtcNow.ToString("yyyyMMdd") + ".log"))
                    crashFile.WriteLine("<crash><time>" + DateTime.UtcNow.TimeOfDay + "</time><url>" +
                                        HttpContext.Current.Request.Url + "</url><exception>" + ex +
                                        "</exception></crash>");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        void SetupApp()
        {
            var bus = new FakeBus();
            ServiceLocator.Report = new ConcurrentDictionary<string, PipelineStatusView.BuildStatus>();
            var statusView = new PipelineStatusView(ServiceLocator.Report);
            bus.RegisterHandler<BuildStarted>(statusView.Handle);
            bus.RegisterHandler<BuildEnded>(statusView.Handle);
            bus.RegisterHandler<BuildUpdated>(statusView.Handle);

            var workingAreaPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            var repoArea = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(repoArea);
            Directory.CreateDirectory(workingAreaPath);

            var fileFinder = new DefaultFileFinder(new PathFinder());
            var pipeline =
                new PipelineOfTasks(
                    new CompoundTaskSource(new MSBuildDetector(fileFinder), new NUnitTaskDetctor(fileFinder)),
                    new ExecTaskGenerator(new ExecTaskFactory()));
            var area = new SubfolderWorkingArea(workingAreaPath);
            var agent = new Agent(bus, new Valve(pipeline, area));
            agent.JoinTheParty();

            ServiceLocator.RepositoryTracker = new RepositoryTracker(bus);
            ServiceLocator.RepositoryTracker.StartListeningForBuildUpdates();
        }
    }
}