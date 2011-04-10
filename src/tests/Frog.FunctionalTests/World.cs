using System;
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
            browser = new FirefoxDriver();
            baseUrl = Environment.GetEnvironmentVariable("ACCEPTANCE_TEST_URL") ?? "http://localhost:6502/";
        }

        [AfterScenario]
        public static void AfterScenario()
        {
            if (ScenarioContext.Current.TestError == null)
            {
                browser.Quit();
            }
        }
    }
}