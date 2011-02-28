using System.IO;
using Frog.Specs.Support;
using Frog.Support;
using NUnit.Framework;

namespace Frog.Domain.Specs.FileGeneration
{
    [TestFixture]
    public class FileGenesisTests
    {
        string workPlace;

        [SetUp]
        public void Setup()
        {
            workPlace = Path.Combine(GitTestSupport.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(workPlace);
        }

        [TearDown]
        public void Cleanup()
        {
            OSHelpers.ClearAttributes(workPlace);
            Directory.Delete(workPlace, true);
        }

        [Test]
        public void should_create_file_with_contents()
        {
            var genesis = new FileGenesis(workPlace);
            genesis.File("Fle.txt", "aaaaa");

            Assert.That(File.ReadAllText(Path.Combine(workPlace, "Fle.txt")), Is.EqualTo("aaaaa"));
        }

        [Test]
        public void should_create_folder_with_a_file()
        {
            var genesis = new FileGenesis(workPlace);
            genesis.Folder("Flo").File("Fle.txt", "aaaaa");

            Assert.That(File.ReadAllText(Path.Combine(workPlace, Os.DirChars("Flo/Fle.txt"))), Is.EqualTo("aaaaa"));
        }

        [Test]
        public void should_create_multiple_files()
        {
            var genesis = new FileGenesis(workPlace);
            genesis.File("Fle.txt", "aaaaa").File("Dugh.bin", "sdsd");

            Assert.That(File.ReadAllText(Path.Combine(workPlace, Os.DirChars("Fle.txt"))), Is.EqualTo("aaaaa"));
            Assert.That(File.ReadAllText(Path.Combine(workPlace, Os.DirChars("Dugh.bin"))), Is.EqualTo("sdsd"));
        }

        [Test]
        public void should_go_up_the_folder_hierarchy()
        {
            var genesis = new FileGenesis(workPlace);
            genesis
                .Folder("down")
                .File("Dugh.bin", "sdsd")
                .Up()
                .File("Fle.txt", "aaaaa");

            Assert.That(File.ReadAllText(Path.Combine(workPlace, Os.DirChars("Fle.txt"))), Is.EqualTo("aaaaa"));
            Assert.That(File.ReadAllText(Path.Combine(workPlace, Os.DirChars("down/Dugh.bin"))), Is.EqualTo("sdsd"));
        }
    }
}