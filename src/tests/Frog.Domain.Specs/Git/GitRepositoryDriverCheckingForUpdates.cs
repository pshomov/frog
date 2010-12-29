using System;
using System.IO;
using Frog.Domain.SourceRepositories;
using NUnit.Framework;

namespace Frog.Domain.Specs
{
    [TestFixture]
    public class GitRepositoryDriverCheckingForUpdates : GitRepositoryDriverCheckBase
    {
        bool _updates;

        public override void Given()
        {
            base.Given();
            _driver.CheckForUpdates();
            GitTestSupport.CommitChange(_testAssemblyPath, "dummy_repo");
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