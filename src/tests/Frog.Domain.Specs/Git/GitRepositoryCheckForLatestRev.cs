using System.IO;
using Frog.Specs.Support;
using NUnit.Framework;

namespace Frog.Domain.Specs.Git
{
    [TestFixture]
    public class GitRepositoryCheckForLatestRev : GitRepositoryDriverCheckBase
    {
        RevisionInfo revision;

        protected override void Given()
        {
            base.Given();
            var changeset = GetChangesetArea();
            var genesis = new FileGenesis(changeset);
            genesis
                .File("SampleProject.sln", "")
                .Folder("src")
                .Folder("tests")
                .Folder("Some.Tests")
                .File("Some.Test.csproj", "");

            GitTestSupport.CommitChangeFiles(repoUrl, changeset);
        }

        protected override void When()
        {
            revision = _driver.GetLatestRevision();
        }

        [Test]
        public void should_return_revision_number()
        {
            Assert.That(revision.Revision.Length == 40, Is.True);
        }

        string GetChangesetArea()
        {
            var changeset = Path.Combine(GitTestSupport.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(changeset);
            return changeset;
        }
    }
}