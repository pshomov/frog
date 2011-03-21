using System;
using System.IO;
using System.Threading;
using Frog.Domain.Specs;
using Frog.Specs.Support;
using Frog.Support;
using NUnit.Framework;
using NUnit.Framework.Constraints;
using OpenQA.Selenium;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.Remote;

namespace Frog.FunctionalTests
{
    [TestFixture]
    public class RegisterProject
    {
        RemoteWebDriver driver;
        string baseUrl;
        string basePath;
		FirefoxDriver driver2;

        [Test]
        public void should_register_project_successfully()
        {
            basePath = GetMeAWorkingFolder();
            string repo = GitTestSupport.CreateDummyRepo(basePath, "testrepo");
			
            string changeset = GetMeAWorkingFolder();
            var gen = new FileGenesis(changeset);
			gen.File("build.sln", "invalid");
			GitTestSupport.CommitChangeFiles(repo, changeset);
			
            driver.Navigate().GoToUrl(U("Content/register.html"));
            driver.FindElement(By.Id("url")).SendKeys(repo);
            driver.FindElementById("reg_button").Click();
            WithRetries(() => driver.FindElementById("newly_registered")).Click();
            driver2 = new FirefoxDriver();
            driver2.Navigate().GoToUrl(U("system/check"));
            AssertThatWithRetries(() => driver.FindElement(By.CssSelector("#status")).Text, Is.EqualTo("Build complete"));			
        }

        string GetMeAWorkingFolder()
        {
            var changeset = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(changeset);
            return changeset;
        }

        static T WithRetries<T>(Func<T> callback, int retries = 100)
        {
            while (true)
            {
                try
                {
                    return callback();
                }
                catch (NoSuchElementException)
                {
                    if (retries > 0)
                    {
                        retries--;
                        Thread.Sleep(100);
                    }
                    else throw;
                }
            }
        }

        static void AssertThatWithRetries<T>(Func<T> callback,  IResolveConstraint constraint, int retries = 100)
        {
            while (true)
            {
                try
                {
                    var val = callback();
                    if (!constraint.Resolve().Matches(ref val))
                    {
                        if (retries > 0)
                        {
                            retries--;
                            Thread.Sleep(100);
                        } else
                        {
                            Assert.That(val, constraint);
                        }
                    } else 
                        return;
                }
                catch (NoSuchElementException)
                {
                    if (retries > 0)
                    {
                        retries--;
                        Thread.Sleep(100);
                    }
                    else throw;
                }
            }
        }

        Uri U(string relativeUrl)
        {
            return new Uri(baseUrl + relativeUrl);
        }

        [SetUp]
        public void Setup()
        {
            baseUrl = "http://localhost:6502/";
            driver = new FirefoxDriver();
        }

        [TearDown]
        public void TearDown()
        {
            driver2.Quit();
            driver.Quit();
            if (!basePath.IsNullOrEmpty())
            {
                OSHelpers.ClearAttributes(basePath);
                Directory.Delete(basePath, true);
            }
        }
    }
}