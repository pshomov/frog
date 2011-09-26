using System;
using System.Collections.Generic;
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

        [Given(@"I have registered project URL ""(.*)""")]
        public void GivenIHaveRegisteredProjectURL(string url)
        {
            GivenIAmOnTheRegistrationPage();
            url = String.Format(url, Environment.GetEnvironmentVariable("MY_GIT_USERNAME"),
                                Environment.GetEnvironmentVariable("MY_GIT_PASSWORD"));
            register_url(url);
            statusPageLink = AssertionHelpers.WithRetries(() => World.browser.FindElementById("newly_registered"));
        }

        [When(@"I am on the status page for project ""(.*)""")]
        [Then(@"I am on the status page for project ""(.*)""")]
        public void WhenIAmOnTheStatusPageForProjectP1(string project)
        {
            World.browser.Navigate().GoToUrl(StatusPages[project]);
        }

        [Given(@"I have a project as ""(.*)""")]
        public void GivenIHave_NETSampleProjectWith1UnitTestingProjectAsP1(string project)
        {
            basePath = OSHelpers.GetMeAWorkingFolder();
            var repo = GitTestSupport.CreateDummyRepo(basePath, "testrepo");
            ScenarioContext.Current[project] = repo;
        }

        [Given(@"I add a test task ""(.*)"" with content ""(.*)"" to project ""(.*)""")]
        [When(@"I add a test task ""(.*)"" with content ""(.*)"" to project ""(.*)""")]
        public void Give_I_add_a_task(string taskName, string content, string project)
        {
            var repo = (string) ScenarioContext.Current[project];
            var gen = new FileGenesis();
            gen.File(string.Format("{0}.testtask", taskName), content.Replace("\\n", Environment.NewLine));
            GitTestSupport.CommitChangeFiles(repo, gen.Root, string.Format("commit on project {0} for adding task {1}", project, taskName));
        }

        [Given(@"I add a test task ""(.*)"" with comment ""(.*)"" and content ""(.*)"" to project ""(.*)""")]
        [When(@"I add a test task ""(.*)"" with comment ""(.*)"" and content ""(.*)"" to project ""(.*)""")]
        public void AddTaskWithComment(string taskName, string comment, string content, string project)
        {
            var repo = (string) ScenarioContext.Current[project];
            var gen = new FileGenesis();
            gen.File(string.Format("{0}.testtask", taskName), content.Replace("\\n", Environment.NewLine));
            GitTestSupport.CommitChangeFiles(repo, gen.Root, comment);
        }

        [Given(@"I type in the url input the ""(.*)"" repository URL and press the 'Register' button")]
        public void GivenITypeInTheUrlInputTheP1RepositoryURLAndPressTheRegisterButton(string project)
        {
            var url = (string) ScenarioContext.Current[project];
            register_url(url);
        }

        void register_url(string url)
        {
            World.browser.FindElement(By.Id("url")).SendKeys(url);
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
        [When(@"I see the build is completed with status (.*)$")]
        public void ThenISeeTheBuildIsCompletedWithStatus(string status)
        {
            var statuses = new Dictionary<string, string>
                               {
                                   {"FAILURE", "Build ended with a failure"},
                                   {"SUCCESS", "Build ended with success"}
                               };
            AssertionHelpers.WithRetries(
                () =>
                Assert.That(World.browser.FindElement(By.CssSelector("#status")).Text, Is.EqualTo(statuses[status])), retries: 300);
        }

        [Then(@"The terminal output has text ""(.*)""")]
        public void ThenTheTerminalOutputHasTextFromTheBuild(string text)
        {
            AssertionHelpers.WithRetries(
                () => Assert.That(World.browser.FindElement(By.CssSelector("#terminal")).Text, Is.EqualTo(text.Replace("\\n", Environment.NewLine))));
        }

        [Then(@"The terminal output contains text ""(.*)""")]
        public void ThenTheTerminalOutputContainsTextFromTheBuild(string text)
        {
            AssertionHelpers.WithRetries(
                () => Assert.That(World.browser.FindElement(By.CssSelector("#terminal")).Text, Is.StringContaining(text.Replace("\\n", Environment.NewLine))), retries: 300);
        }

        [Then(@"I see build history contains (.*) items")]
        public void BuildHistoryContains(int itemCount)
        {
            AssertionHelpers.WithRetries(
                () => Assert.That(World.browser.FindElements(By.CssSelector("#history li")).Count, Is.EqualTo(itemCount)), retries: 300);
        }
        [Then(@"I see build history item (.*) contains ""(.*)""")]
        public void BuildHistoryItemContains(int itemIndex, string contains)
        {
            AssertionHelpers.WithRetries(
                () => Assert.That(World.browser.FindElements(By.CssSelector("#history li"))[itemIndex-1].Text, Is.StringContaining(contains)), retries: 300);
        }

        Uri U(string relativeUrl)
        {
            return new Uri(World.baseUrl + relativeUrl);
        }
    }
}