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
        string repo;
        IWebElement statusPageLink;

        [Given(@"I have a \.NET simple non-working project")]
        public void GivenIHaveA_NETSimpleNonWorkingProject()
        {
            basePath = OSHelpers.GetMeAWorkingFolder();
            repo = GitTestSupport.CreateDummyRepo(basePath, "testrepo");

            var gen = new FileGenesis();
            gen.File("build.sln", "invalid");
            GitTestSupport.CommitChangeFiles(repo, gen.Root);
        }

        [Given(@"I am on the ""registration"" page")]
        public void GivenIAmOnTheRegistrationPage()
        {
            World.browser.Navigate().GoToUrl(U("Content/register.html"));
        }

        [Given(@"I type in the url input the project's repository URL and press the 'Register' button")]
        public void GivenITypeInTheUrlInputTheProjectSRepositoryURL_and_press_the_Register_button()
        {
            World.browser.FindElement(By.Id("url")).SendKeys(repo);
            World.browser.FindElementById("reg_button").Click();
        }

        [Then(@"I get a link to the status page for the project")]
        public void ThenIGetALinkToTheStatusPageForTheProject()
        {
            AssertionHelpers.WithRetries(() => World.browser.FindElementById("newly_registered"));
        }

        [Given(@"I have registered the project")]
        public void GivenIHaveRegisteredTheProject()
        {
            GivenIAmOnTheRegistrationPage();
            GivenITypeInTheUrlInputTheProjectSRepositoryURL_and_press_the_Register_button();
            statusPageLink = AssertionHelpers.WithRetries(() => World.browser.FindElementById("newly_registered"));
        }

        [When(@"I check for updates")]
        public void WhenICheckForUpdates()
        {
            var browser2 = new FirefoxDriver();
            browser2.Navigate().GoToUrl(U("system/check"));
            browser2.Quit();
        }

        [When(@"I am on the status page for the project")]
        public void WhenIAmOnTheStatusPageForTheProject()
        {
            statusPageLink.Click();
        }

        [Then(@"I see the build is completed with status (.*)$")]
        public void ThenISeeTheBuildIsCompletedWithStatus(string status)
        {
            var statuses = new Dictionary<string, string>() {{"FAILURE", "Build ended with a failure"}};
            AssertionHelpers.WithRetries(
                () => Assert.That(World.browser.FindElement(By.CssSelector("#status")).Text, Is.EqualTo(statuses[status])));
        }

        [Then(@"The terminal output contains text from the build")]
        public void ThenTheTerminalOutputContainsTextFromTheBuild()
        {
            AssertionHelpers.WithRetries(
                () => Assert.That(World.browser.FindElement(By.CssSelector("#terminal")).Text.Length, Is.GreaterThan(2)));
        }

        Uri U(string relativeUrl)
        {
            return new Uri(World.baseUrl + relativeUrl);
        }
    }
}