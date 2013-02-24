using System;
using System.Linq;
using Frog.Domain.Integration;
using Frog.Support;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.Remote;
using TechTalk.SpecFlow;

namespace Frog.FunctionalTests
{
    [Binding]
    public static class World
    {
        public static RemoteWebDriver browser;
        public static string baseUrl;

        [BeforeScenario]
        public static void BeforeScenario()
        {
            browser = new ChromeDriver();
            baseUrl = Environment.GetEnvironmentVariable("ACCEPTANCE_TEST_URL") ?? "http://localhost:1169/";
        }

        [AfterScenario]
        public static void AfterScenario()
        {
            if (ScenarioContext.Current.TestError == null)
            {
                browser.Quit();
            }
        }

        [BeforeTestRun]
        public static void BeforeTestRun()
        {
            RemoveAllProjects();
        }

        [AfterScenario]
        public static void AfterScenarioProjectCleanUp()
        {
            if (FeatureContext.Current.FeatureInfo.Tags == null || !FeatureContext.Current.FeatureInfo.Tags.Contains("no_clean")) RemoveAllProjects(); 
        }

        private static void RemoveAllProjects()
        {
			Console.WriteLine("Wiping out the data store ....");
            var riak = new RiakProjectRepository(
                OSHelpers.RiakHost(),
                OSHelpers.RiakPort(),
                "projects");
            riak.AllProjects.ToList().ForEach(document => riak.RemoveProject(document.projecturl));
        }
    }
}