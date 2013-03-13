using System.Collections.Generic;
using System.IO;
using Frog.Domain.Integration;
using Frog.Specs.Support;
using Frog.Support;
using NUnit.Framework;

namespace Frog.Domain.IntegrationTests.PathGoble
{
    [TestFixture]
    public class GobleTests
    {
        List<string> list;
        PathFinder pathFinder;
        string workPlace;

        [SetUp]
        public void Setup()
        {
            list = new List<string>();
            pathFinder = new PathFinder();

            workPlace = Path.Combine(GitTestSupport.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(workPlace);
            var genesis = new FileGenesis(workPlace);
            genesis.Folder("TestFixtures")
                .File("notest.txt", "")
                .File("test.txt", "")
                .Folder("Goble")
                    .File("l0f1.txt", "")
                    .File("l0f2.txt", "")
                    .Folder("level1")
                        .File("l1f1.txt", "")
                        .File("l1f2.txt", "")
                        .Folder("level2")
                            .File("l2f1.txt", "")
                            .File("l2f2.txt", "");


        }

        [TearDown]
        public void TearDown()
        {
            Directory.Delete(workPlace, true);
        }

        [Test]
        public void should_find_file_down_a_folder_hieararchy()
        {
            pathFinder.FindFilesRecursively(s => list.Add(s.Remove(0, workPlace.Length+1)), "l1f1.txt", Path.Combine(workPlace, "TestFixtures"));

            Assert.That(list.Count, Is.EqualTo(1));
            Assert.That(list[0],
                        Is.EqualTo(Os.DirChars("TestFixtures/Goble/level1/l1f1.txt")));
        }

        [Test]
        public void should_find_multiple_files_at_the_same_level_down_the_hierarchy()
        {
            pathFinder.FindFilesRecursively(s => list.Add(s.Remove(0, workPlace.Length + 1)), "l2*.txt", Path.Combine(workPlace, "TestFixtures"));

            Assert.That(list.Count, Is.EqualTo(2));
            Assert.That(list[0],
                        Is.EqualTo(Os.DirChars("TestFixtures/Goble/level1/level2/l2f1.txt")));
            Assert.That(list[1],
                        Is.EqualTo(Os.DirChars("TestFixtures/Goble/level1/level2/l2f2.txt")));
        }

        [Test]
        public void should_find_multiple_files_at_different_levels_down_the_hierarchy()
        {
            pathFinder.FindFilesRecursively(s => list.Add(s.Remove(0, workPlace.Length + 1)), "l?f*.txt", Path.Combine(workPlace, "TestFixtures"));

            Assert.That(list.Count, Is.EqualTo(6));
            Assert.That(list[0],
                        Is.EqualTo(Os.DirChars("TestFixtures/Goble/level1/level2/l2f1.txt")));
            Assert.That(list[1],
                        Is.EqualTo(Os.DirChars("TestFixtures/Goble/level1/level2/l2f2.txt")));
            Assert.That(list[2],
                        Is.EqualTo(Os.DirChars("TestFixtures/Goble/level1/l1f1.txt")));
            Assert.That(list[3],
                        Is.EqualTo(Os.DirChars("TestFixtures/Goble/level1/l1f2.txt")));
            Assert.That(list[4],
                        Is.EqualTo(Os.DirChars("TestFixtures/Goble/l0f1.txt")));
            Assert.That(list[5],
                        Is.EqualTo(Os.DirChars("TestFixtures/Goble/l0f2.txt")));
        }

        [Test]
        public void should_look_for_files_only_in_the_root_of_the_hierarchy()
        {
            pathFinder.FindFilesAtTheBase(s => list.Add(s.Remove(0, workPlace.Length + 1)), "l?f*.txt", Path.Combine(workPlace, "TestFixtures"));
            Assert.That(list.Count, Is.EqualTo(0));
        }

        [Test]
        public void should_find_files_that_are_at_the_root_of_the_hierarchy()
        {
            pathFinder.FindFilesAtTheBase(s => list.Add(s.Remove(0, workPlace.Length + 1)), "*.txt", Path.Combine(workPlace, "TestFixtures"));
            Assert.That(list.Count, Is.EqualTo(2));
            Assert.That(list[0],
                        Is.EqualTo(Os.DirChars("TestFixtures/notest.txt")));
            Assert.That(list[1],
                        Is.EqualTo(Os.DirChars("TestFixtures/test.txt")));
        }
    }
}