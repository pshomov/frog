﻿using System;
using System.Collections.Concurrent;
using System.IO;
using System.Web.Mvc;
using System.Web.Routing;
using Frog.Domain.Integration;
using Frog.Support;
using Frog.WiredUp;
using Lokad.Cqrs;
using Lokad.Cqrs.AtomicStorage;
using SaaS.Client.Projections.Releases;
using SaaS.Wires;
using SimpleCQRS;

namespace Frog.UI.Web
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class RunzFrontendApplication : System.Web.HttpApplication
    {
        string mapPath;
        string serviceArea;

        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }

        protected void Application_Start()
        {
            serviceArea = Server.MapPath("~/App_Data");
            Profiler.MeasurementsBridge = new Profiler.LogFileLoggingBridge("runz_web_ui.log");
            Directory.CreateDirectory(serviceArea);
            mapPath = Path.Combine(serviceArea, "Crash_");

            AppDomain.CurrentDomain.UnhandledException += CurrentDomainOnUnhandledException;
            AreaRegistration.RegisterAllAreas();

            RegisterGlobalFilters(GlobalFilters.Filters);
            WireUpUIModelInfrastructure();

            WebFrontendSetup frontendSetup = GetWebFrontendSetup();
            frontendSetup.RegisterRoutes(RouteTable.Routes);
        }

        WebFrontendSetup GetWebFrontendSetup()
        {
            string mode = Environment.GetEnvironmentVariable("RUNZ_ACCEPTANCE_MODE");
            WebFrontendSetup setup;
//            if (!mode.IsNullOrEmpty() && mode == "ACCEPTANCE")
                setup = new AcceptanceTestWebFrontendSetup();
//            else
//                setup = new WebFrontendSetup();
            return setup;
        }

        void CurrentDomainOnUnhandledException(object sender, UnhandledExceptionEventArgs unhandledExceptionEventArgs)
        {
            var ex = (Exception)unhandledExceptionEventArgs.ExceptionObject;
            try
            {
                Console.WriteLine("Exception is: \n" + ex);
                using (var crashFile = new StreamWriter(mapPath + DateTime.UtcNow.ToString("yyyyMMdd") + ".log", true))
                    crashFile.WriteLine("<crash><exception>" + ex +
                                        "</exception></crash>");
            }
            catch (Exception e)
            {
                Console.WriteLine(string.Format("Storing information of unhandled exception failed. Here is the exception that stopped the processing of the original one:{0}{1}And here is the original exception:{2}", e, Environment.NewLine, ex));
            }
        }

        void WireUpUIModelInfrastructure()
        {
            ServiceLocator.Bus = new RabbitMQBus(OSHelpers.RabbitHost());
            ServiceLocator.Store = Setup.GetDocumentStore(OSHelpers.LokadStorePath(), Setup.OpensourceCustomer);
        }
    }
    internal class WebFrontendSetup
    {
        public virtual void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute("codebase_task_all_terminal_output", "project/codebase/{company}/{project}/{repository}/task",
                            new { controller = "CodebaseProject", action = "allterminaloutput" });
            routes.MapRoute("codebase_task_terminal_output", "project/codebase/{company}/{project}/{repository}/task/{taskIndex}",
                            new { controller = "CodebaseProject", action = "terminaloutput" });
            routes.MapRoute("codebase_status", "project/codebase/{company}/{project}/{repository}/{action}",
                            new { controller = "CodebaseProject" });

            routes.MapRoute("task_all_terminal_output", "project/github/{user}/{project}/task",
                            new { controller = "Project", action = "allterminaloutput" });
            routes.MapRoute("task_terminal_output", "project/github/{user}/{project}/task/{taskIndex}",
                            new { controller = "Project", action = "terminaloutput" });
            routes.MapRoute("github_status", "project/github/{user}/{project}/{action}",
                            new { controller = "Project" });
            routes.MapRoute("github_register_project", "project/register",
                            new { controller = "RegisterProject", action = "index" });
        }
    }

    internal class AcceptanceTestWebFrontendSetup : WebFrontendSetup
    {
        public override void RegisterRoutes(RouteCollection routes)
        {
            routes.MapRoute("test_all_task_terminal_output", "project/test/file/{projectUrl}/task",
                            new { controller = "TestProject", action = "allterminaloutput" });
            routes.MapRoute("test_task_terminal_output", "project/test/file/{projectUrl}/task/{taskIndex}",
                            new { controller = "TestProject", action = "terminaloutput" });
            routes.MapRoute("test_repo_status", "project/test/file/{projectUrl}/{action}",
                            new { controller = "TestProject" });
            routes.MapRoute("test_register_project", "project/register",
                            new { controller = "TestRegisterProject", action = "Index" });
            base.RegisterRoutes(routes);
        }
    }
}