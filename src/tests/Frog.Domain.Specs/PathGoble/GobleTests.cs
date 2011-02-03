using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
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
                new PathFinder(AppDomain.CurrentDomain.BaseDirectory, "l1f1.txt");

            pathFinder.apply(list.Add);

            Assert.That(list.Count, Is.EqualTo(1));
            Assert.That(list[0], Is.EqualTo(string.Format("{1}{0}TestFixtures{0}Goble{0}level1{0}l1f1.txt", Path.DirectorySeparatorChar,
                                             AppDomain.CurrentDomain.BaseDirectory)));
        }

        [Test]
        public void should_find_multiple_files_at_the_same_level_down_the_hierarchy()
        {
            var list = new List<string>();
            var pathFinder =
                new PathFinder(AppDomain.CurrentDomain.BaseDirectory, "l2*.txt");

            pathFinder.apply(list.Add);

            Assert.That(list.Count, Is.EqualTo(2));
            Assert.That(list[0], Is.EqualTo(string.Format("{1}{0}TestFixtures{0}Goble{0}level1{0}level2{0}l2f1.txt", Path.DirectorySeparatorChar,
                                             AppDomain.CurrentDomain.BaseDirectory)));
            Assert.That(list[1], Is.EqualTo(string.Format("{1}{0}TestFixtures{0}Goble{0}level1{0}level2{0}l2f2.txt", Path.DirectorySeparatorChar,
                                             AppDomain.CurrentDomain.BaseDirectory)));
        }

        [Test]
        public void should_find_multiple_files_at_different_levels_down_the_hierarchy()
        {
            var list = new List<string>();
            var pathFinder =
                new PathFinder(AppDomain.CurrentDomain.BaseDirectory, "l?f*.txt");

            pathFinder.apply(list.Add);

            Assert.That(list.Count, Is.EqualTo(6));
            Assert.That(list[0], Is.EqualTo(string.Format("{1}{0}TestFixtures{0}Goble{0}level1{0}level2{0}l2f1.txt", Path.DirectorySeparatorChar,
                                             AppDomain.CurrentDomain.BaseDirectory)));
            Assert.That(list[1], Is.EqualTo(string.Format("{1}{0}TestFixtures{0}Goble{0}level1{0}level2{0}l2f2.txt", Path.DirectorySeparatorChar,
                                             AppDomain.CurrentDomain.BaseDirectory)));
            Assert.That(list[2], Is.EqualTo(string.Format("{1}{0}TestFixtures{0}Goble{0}level1{0}l1f1.txt", Path.DirectorySeparatorChar,
                                             AppDomain.CurrentDomain.BaseDirectory)));
            Assert.That(list[3], Is.EqualTo(string.Format("{1}{0}TestFixtures{0}Goble{0}level1{0}l1f2.txt", Path.DirectorySeparatorChar,
                                             AppDomain.CurrentDomain.BaseDirectory)));
            Assert.That(list[4], Is.EqualTo(string.Format("{1}{0}TestFixtures{0}Goble{0}l0f1.txt", Path.DirectorySeparatorChar,
                                             AppDomain.CurrentDomain.BaseDirectory)));
            Assert.That(list[5], Is.EqualTo(string.Format("{1}{0}TestFixtures{0}Goble{0}l0f2.txt", Path.DirectorySeparatorChar,
                                             AppDomain.CurrentDomain.BaseDirectory)));
        }
    }

    public class PathFinder
    {
        string start;
        readonly string pattern;

        public PathFinder(string start, string pattern)
        {
            this.start = start;
            this.pattern = pattern;
        }

        public void apply(Action<string> action)
        {
            Diver(start,  action); 
        }

        void Diver(string currentDir, Action<string> action)
        {
            if (Directory.Exists(currentDir))
            {
                string[] subDirs = Directory.GetDirectories(currentDir);
                foreach (string dir in subDirs)
                {
                    Diver(dir, action);
                }
                string[] files = Directory.GetFiles(currentDir, pattern);
                foreach (string file in files)
                    action(file);
            }
        }
    }
}