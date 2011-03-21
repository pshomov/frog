using System;
using System.IO;
using System.Threading;
using Frog.Domain.Specs;
using Frog.Specs.Support;
using Frog.Support;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.Remote;

namespace Frog.FunctionalTests
{
    [TestFixture]
    public class RegisterProject
    {
        RemoteWebDriver browser1;
        FirefoxDriver browser2;
        string baseUrl;
        string basePath;

        [Test]
        public void should_register_project_successfully()
        {
            basePath = OSHelpers.GetMeAWorkingFolder();
            string repo = GitTestSupport.CreateDummyRepo(basePath, "testrepo");

            var gen = new FileGenesis();
            gen.File("build.sln", "invalid");
            GitTestSupport.CommitChangeFiles(repo, gen.Root);

            browser1.Navigate().GoToUrl(U("Content/register.html"));
            browser1.FindElement(By.Id("url")).SendKeys(repo);
            browser1.FindElementById("reg_button").Click();
            WithRetries(() => browser1.FindElementById("newly_registered")).Click();
            browser2 = new FirefoxDriver();
            browser2.Navigate().GoToUrl(U("system/check"));
            browser2.Quit();
            WithRetries(
                () => Assert.That(browser1.FindElement(By.CssSelector("#status")).Text, Is.EqualTo("Build complete")));
        }


        static T WithRetries<T>(Func<T> query, int retries = 100)
        {
            while (true)
            {
                try
                {
                    return query();
                }
                catch (NoSuchElementException)
                {
                    if (retries > 0)
                    {
                        retries = SleepABit(retries);
                    }
                    else throw;
                }
            }
        }

        static void WithRetries(Action action, int retries = 100)
        {
            while (true)
            {
                try
                {
                    action();
                    return;
                }
                catch (AssertionException)
                {
                    if (retries > 0)
                    {
                        retries = SleepABit(retries);
                    }
                    else throw;
                }
                catch (NoSuchElementException)
                {
                    if (retries > 0)
                    {
                        retries = SleepABit(retries);
                    }
                    else throw;
                }
            }
        }

        static int SleepABit(int retries)
        {
            retries--;
            Thread.Sleep(100);
            return retries;
        }

        Uri U(string relativeUrl)
        {
            return new Uri(baseUrl + relativeUrl);
        }

        [SetUp]
        public void Setup()
        {
            baseUrl = "http://localhost:6502/";
            browser1 = new FirefoxDriver();
        }

        [TearDown]
        public void TearDown()
        {
            browser1.Quit();
            if (!basePath.IsNullOrEmpty())
            {
                OSHelpers.ClearAttributes(basePath);
                Directory.Delete(basePath, true);
            }
        }
    }
}