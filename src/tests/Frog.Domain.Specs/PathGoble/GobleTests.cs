using System.Collections.Generic;
using Frog.Support;
using NUnit.Framework;

namespace Frog.Domain.Specs.PathGoble
{
    [TestFixture]
    public class GobleTests
    {
        [Test]
        public void should_find_file_down_a_folder_hieararchy()
        {
            var list = new List<string>();
            var pathFinder =
                new PathFinder();

            pathFinder.apply(list.Add, "l1f1.txt", "TestFixtures");

            Assert.That(list.Count, Is.EqualTo(1));
            Assert.That(list[0],
                        Is.EqualTo(Os.DirChars("TestFixtures/Goble/level1/l1f1.txt")));
        }

        [Test]
        public void should_find_multiple_files_at_the_same_level_down_the_hierarchy()
        {
            var list = new List<string>();
            var pathFinder =
                new PathFinder();

            pathFinder.apply(list.Add, "l2*.txt", "TestFixtures");

            Assert.That(list.Count, Is.EqualTo(2));
            Assert.That(list[0],
                        Is.EqualTo(Os.DirChars("TestFixtures/Goble/level1/level2/l2f1.txt")));
            Assert.That(list[1],
                        Is.EqualTo(Os.DirChars("TestFixtures/Goble/level1/level2/l2f2.txt")));
        }

        [Test]
        public void should_find_multiple_files_at_different_levels_down_the_hierarchy()
        {
            var list = new List<string>();
            var pathFinder =
                new PathFinder();

            pathFinder.apply(list.Add, "l?f*.txt", "TestFixtures");

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
    }
}