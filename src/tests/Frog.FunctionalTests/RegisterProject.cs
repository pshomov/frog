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
        RemoteWebDriver driver;
        string baseUrl;
        string basePath;
		FirefoxDriver driver2;

        [Test]
        public void should_register_project_successfully()
        {
            basePath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(basePath);
            string repo = GitTestSupport.CreateDummyRepo(basePath, "testrepo");
			
            var changeset = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(changeset);
			var gen = new FileGenesis(changeset);
			gen.File("build.sln", "invalid");
			GitTestSupport.CommitChangeFiles(repo, changeset);
			
            driver.Navigate().GoToUrl(U("Content/register.html"));
            driver.FindElement(By.Id("url")).SendKeys(repo);
            driver.FindElementById("reg_button").Click();
            driver2 = new FirefoxDriver();
            driver2.Navigate().GoToUrl(U("system/check"));
			driver2.Quit();
            WithRetries(() => driver.FindElementById("newly_registered")).Click();
			Assert.That(WithRetries(() => driver.FindElement(By.CssSelector("#status")).Text), Is.EqualTo("Build complete"));			
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
            driver.Quit();
            if (!basePath.IsNullOrEmpty())
            {
                OSHelpers.ClearAttributes(basePath);
                Directory.Delete(basePath, true);
            }
        }
    }
}