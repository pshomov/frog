using System;
using System.IO;
using Frog.Specs.Support;
using Frog.Support;
using NUnit.Framework;

namespace Frog.Domain.Specs.Git
{
    [TestFixture]
    public class GitRepositoryCheckForLatestRev : GitRepositoryDriverCheckBase
    {
        string revision;

        public override void Given()
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
        }

        public override void When()
        {
            revision = _driver.GetLatestRevision(repoUrl);
        }

        [Test]
        public void should_return_revision_number()
        {
            Assert.That(revision.Length == 40, Is.True);
        }

        string GetChangesetArea()
        {
            var changeset = Path.Combine(GitTestSupport.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(changeset);
            return changeset;
        }

    }
}