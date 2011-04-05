﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Frog.Domain;
using Frog.Domain.TaskSources;
using Frog.Support;
using SimpleCQRS;

namespace Frog.UI.Web
{
    public class MvcApplication : HttpApplication
    {
        string mapPath;

        protected void Application_Start()
        {
            var exceptionDumpster = Server.MapPath("~/App_Data");
            Directory.CreateDirectory(exceptionDumpster);
            mapPath = Path.Combine(exceptionDumpster, "Crash_");

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
            var ex = (Exception) unhandledExceptionEventArgs.ExceptionObject;
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

        void WireUpApp()
        {
            var system = new ProductionSystem();
            ServiceLocator.RepositoryTracker = system.repositoryTracker;
            ServiceLocator.Report = system.report;
            ServiceLocator.AllMassages = system.AllMessages;
        }
    }

    internal class WebFrontendSetup
    {
        public virtual void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute("diagnostics_all_messages", "diagnostics/messages",
                            new { controller = "Diagnostics", action = "allmessages" });
            routes.MapRoute("task_all_terminal_output", "project/github/{user}/{project}/task",
                            new { controller = "Project", action = "allterminaloutput" });
            routes.MapRoute("task_terminal_output", "project/github/{user}/{project}/task/{taskIndex}",
                            new { controller = "Project", action = "terminaloutput" });
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
            routes.MapRoute("test_all_task_terminal_output", "project/test/file/{projectUrl}/task",
                            new {controller = "TestProject", action = "allterminaloutput"});
            routes.MapRoute("test_task_terminal_output", "project/test/file/{projectUrl}/task/{taskIndex}",
                            new {controller = "TestProject", action = "terminaloutput"});
            routes.MapRoute("test_repo_status", "project/test/file/{projectUrl}/{action}",
                            new {controller = "TestProject"});
            routes.MapRoute("test_register_project", "project/register",
                            new {controller = "TestRegisterProject", action = "Index"});
            base.RegisterRoutes(routes);
        }
    }

    public class ProductionSystem : SystemBase
    {
        IBusDebug debug;
        public readonly ConcurrentQueue<Message> AllMessages = new ConcurrentQueue<Message>();
        public ProductionSystem()
        {
            debug = (IBusDebug) TheBus;
            debug.OnMessage += message => AllMessages.Enqueue(message);
        }

        protected override WorkingAreaGoverner SetupWorkingAreaGovernor()
        {
            var workingAreaPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(workingAreaPath);
            return new SubfolderWorkingAreaGoverner(workingAreaPath);
        }

        protected override PipelineOfTasks GetPipeline()
        {
            var fileFinder = new DefaultFileFinder(new PathFinder());
            return new PipelineOfTasks(new CompoundTaskSource(
                                           new TestTaskDetector(fileFinder),
                                           new MSBuildDetector(fileFinder),
                                           new NUnitTaskDetctor(fileFinder)
                                           ),
                                       new ExecTaskGenerator(new ExecTaskFactory()));
        }

    }
}