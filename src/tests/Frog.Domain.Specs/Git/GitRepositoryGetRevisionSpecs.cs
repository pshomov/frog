using System;
using System.IO;
using Frog.Specs.Support;
using Frog.Support;
using NUnit.Framework;

namespace Frog.Domain.Specs.Git
{
    [TestFixture]
    public class GitRepositoryGetLatestRev : GitRepositoryDriverCheckBase
    {
        RevisionInfo revision1;
        RevisionInfo revision2;
        string workingArea;

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
            changeset = GetChangesetArea();
            genesis = new FileGenesis(changeset);
            genesis
                .File("Fle.txt", "f");
            GitTestSupport.CommitChangeFiles(repoUrl, changeset);
            revision2 = _driver.GetLatestRevision();

        }

        protected override void When()
        {
            workingArea = Path.Combine(GitTestSupport.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(workingArea);
            _driver.GetSourceRevision(revision1.Revision, workingArea );
        }

        [Test]
        public void should_have_different_revisions()
        {
            Assert.That(revision1,  Is.Not.SameAs(revision2));
        }

        [Test]
        public void should_checkout_the_older_revision()
        {
            Assert.That(!File.Exists(Path.Combine(workingArea, "Fle.txt")));
        }


        string GetChangesetArea()
        {
            var changeset = Path.Combine(GitTestSupport.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(changeset);
            return changeset;
        }

    }
}