using System;
using System.Collections.Generic;
using Frog.Domain.Specs;
using Frog.Specs.Support;
using Frog.Support;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Firefox;
using TechTalk.SpecFlow;

namespace Frog.FunctionalTests
{
    [Binding]
    public class StepDefinitions
    {
        string basePath;
        IWebElement statusPageLink;

        public Dictionary<string, string> StatusPages
        {
            get
            {
                if (!ScenarioContext.Current.ContainsKey("status_pages"))
                    ScenarioContext.Current["status_pages"] = new Dictionary<string, string>();
                return (Dictionary<string, string>) ScenarioContext.Current["status_pages"];
            }
        }

        static string GetFullURLfromRelative(string relative)
        {
            if (relative.StartsWith("/"))
            {
                if (World.baseUrl.EndsWith("/"))
                    return World.baseUrl.Remove(World.baseUrl.Length - 1) + relative;
                return World.baseUrl + relative;
            }
            string url = World.browser.Url;
            return url.Substring(0, url.LastIndexOf('/')) + relative;
        }

        [Given(@"I have registered project ""(.*)""")]
        public void GivenIHaveRegisteredProjectP1(string project)
        {
            GivenIAmOnTheRegistrationPage();
            GivenITypeInTheUrlInputTheP1RepositoryURLAndPressTheRegisterButton(project);
            statusPageLink = AssertionHelpers.WithRetries(() => World.browser.FindElementById("newly_registered"));
            string target = statusPageLink.GetAttribute("href");
            StatusPages[project] = GetFullURLfromRelative(target);
        }

        [When(@"I am on the status page for project ""(.*)""")]
        [Then(@"I am on the status page for project ""(.*)""")]
        public void WhenIAmOnTheStatusPageForProjectP1(string project)
        {
            World.browser.Navigate().GoToUrl(StatusPages[project]);
        }

        [Given(@"I have \.NET sample project with (.*) testing project as ""(.*)""")]
        public void GivenIHave_NETSampleProjectWith1UnitTestingProjectAsP1(int numberOfTestTasks, string project)
        {
            basePath = OSHelpers.GetMeAWorkingFolder();
            var repo = GitTestSupport.CreateDummyRepo(basePath, "testrepo");

            var gen = new FileGenesis();
            for (int i = 0; i < numberOfTestTasks; i++)
            {
                gen.File(string.Format("t{0}.testtask", i), string.Format("sample output from task {0} on project {1}", i+1, project));
            }
            GitTestSupport.CommitChangeFiles(repo, gen.Root, string.Format("commit on project {0} with number of tasks {1}", project, numberOfTestTasks));
            ScenarioContext.Current[project] = repo;
        }

        [Given(@"I type in the url input the ""(.*)"" repository URL and press the 'Register' button")]
        public void GivenITypeInTheUrlInputTheP1RepositoryURLAndPressTheRegisterButton(string project)
        {
            World.browser.FindElement(By.Id("url")).SendKeys((string) ScenarioContext.Current[project]);
            World.browser.FindElementById("reg_button").Click();
        }

        [Given(@"I am on the ""registration"" page")]
        public void GivenIAmOnTheRegistrationPage()
        {
            World.browser.Navigate().GoToUrl(U("Content/register.html"));
        }

        [Then(@"I get a link to the status page for the project")]
        public void ThenIGetALinkToTheStatusPageForTheProject()
        {
            AssertionHelpers.WithRetries(() => World.browser.FindElementById("newly_registered"));
        }

        [When(@"I check for updates")]
        public void WhenICheckForUpdates()
        {
            var browser2 = new FirefoxDriver();
            browser2.Navigate().GoToUrl(U("system/check"));
            browser2.Quit();
        }

        [Then(@"I see the build is completed with status (.*)$")]
        public void ThenISeeTheBuildIsCompletedWithStatus(string status)
        {
            var statuses = new Dictionary<string, string>
                               {
                                   {"FAILURE", "Build ended with a failure"},
                                   {"SUCCESS", "Build ended with success"}
                               };
            AssertionHelpers.WithRetries(
                () =>
                Assert.That(World.browser.FindElement(By.CssSelector("#status")).Text, Is.EqualTo(statuses[status])));
        }

        [Then(@"The terminal output contains text ""(.*)""")]
        public void ThenTheTerminalOutputContainsTextFromTheBuild(string text)
        {
            AssertionHelpers.WithRetries(
                () => Assert.That(World.browser.FindElement(By.CssSelector("#terminal")).Text, Is.EqualTo(text.Replace("\\n", " "))));
        }

        Uri U(string relativeUrl)
        {
            return new Uri(World.baseUrl + relativeUrl);
        }
    }
}