using System;
using System.IO;
using Frog.Support;
using NUnit.Framework;

namespace Frog.Domain.Specs.Git
{
    [TestFixture]
    public class GitRepositoryDriverCheckingForUpdates : GitRepositoryDriverCheckBase
    {
        bool _updates;

        public override void Given()
        {
            base.Given();
            _driver.CheckForUpdates();
            GitTestSupport.CommitChangeFiles(repoUrl, Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Os.DirChars("TestFixtures")));
        }

        public override void When()
        {
            _updates = _driver.CheckForUpdates();
        }

        [Test]
        public void should_report_that_there_are_updates()
        {
            Assert.That(_updates);
        }
    }
}