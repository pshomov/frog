using System;
using System.IO;
using Frog.Specs.Support;
using Frog.Support;
using NUnit.Framework;

namespace Frog.Domain.Specs.Git
{
    [TestFixture]
    public class GitRepositoryGetSourceRev : GitRepositoryDriverCheckBase
    {
        RevisionInfo revision1;
        string workingArea;
        private CheckoutInfo sourceRevision;

        protected override void Given()
        {
            base.Given();
            var changeset = GetChangesetArea();
            var genesis = new FileGenesis(changeset);
            genesis
                .Folder("src")
                    .Folder("tests")
                        .Folder("Some.Tests")
                            .File("Some.Test.csproj", "")
                            .Up()
                        .Up()
                    .Up()
                .File("SampleProject.sln", "");

            GitTestSupport.CommitChangeFiles(repoUrl, changeset);
            revision1 = _driver.GetLatestRevision();
        }

        protected override void When()
        {
            workingArea = Path.Combine(GitTestSupport.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(workingArea);
            sourceRevision = _driver.GetSourceRevision(revision1.Revision, workingArea );
        }

        [Test]
        public void should_have_the_revision_number_in_the_checkout_info()
        {
            Assert.That(sourceRevision.Revision, Is.EqualTo(revision1.Revision));
        }

        string GetChangesetArea()
        {
            var changeset = Path.Combine(GitTestSupport.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(changeset);
            return changeset;
        }

    }
}