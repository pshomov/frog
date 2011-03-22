using System;
using System.IO;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Frog.Domain;
using Frog.Support;

namespace Frog.UI.Web
{
    public class MvcApplication : HttpApplication
    {
        string mapPath;

        protected void Application_Start()
        {
            mapPath = Server.MapPath("~/App_Data/Crash_");
            AppDomain.CurrentDomain.UnhandledException += CurrentDomainOnUnhandledException;
            AreaRegistration.RegisterAllAreas();

            WireUpApp();

            WebFrontendSetup frontendSetup = GetWebFrontendSetup();
            frontendSetup.RegisterRoutes(RouteTable.Routes);
        }

        WebFrontendSetup GetWebFrontendSetup()
        {
            string mode = Environment.GetEnvironmentVariable("RUNZ_ACCEPTANCE_MODE");
            WebFrontendSetup setup;
            if (!mode.IsNullOrEmpty() && mode == "ACCEPTANCE")
                setup = new AcceptanceTestWebFrontendSetup();
            else 
                setup = new WebFrontendSetup();
            return setup;
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

        void WireUpApp()
        {
            var system = new ProductionSystem();
            ServiceLocator.RepositoryTracker = system.repositoryTracker;
            ServiceLocator.Report = system.report;
        }
    }

    internal class WebFrontendSetup
    {
        public virtual void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute("github_status", "project/github/{user}/{project}/{action}",
                            new {controller = "Project"});
            routes.MapRoute("github_register_project", "project/register",
                            new {controller = "RegisterProject", action = "index"});
            routes.MapRoute("check_projects_for_updates", "system/check",
                            new {controller = "System", action = "check"});
        }
    }

    internal class AcceptanceTestWebFrontendSetup : WebFrontendSetup
    {
        public override void RegisterRoutes(RouteCollection routes)
        {
            routes.MapRoute("test_repo_status", "project/test/file/{projectUrl}/{action}",
                            new {controller = "TestProject"});
            routes.MapRoute("test_register_project", "project/register",
                            new {controller = "TestRegisterProject", action = "Index"});
            base.RegisterRoutes(routes);
        }
    }

    public class ProductionSystem : SystemBase
    {
        protected override WorkingArea SetupWorkingArea()
        {
            var workingAreaPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(workingAreaPath);
            return new SubfolderWorkingArea(workingAreaPath);
        }

        protected override ExecTaskFactory GetExecTaskFactory()
        {
            return new ExecTaskFactory();
        }
    }
}e