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
        public void should_accept_single_asterisk_surrounded_by_slashes_and_interpret_it_as_one_level_folder_wide_card()
        {
            var list = new List<string>();
            var pathFinder =
                new PathFinder(AppDomain.CurrentDomain.BaseDirectory, "*l1f1.txt");

            pathFinder.apply(f => list.Add(f));

            Assert.That(list.Count, Is.EqualTo(1));
            Assert.That(list[0], Is.EqualTo(string.Format("{1}{0}TestFixtures{0}Goble{0}level1{0}l1f1.txt", Path.DirectorySeparatorChar,
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
            Diver(start, pattern,  action); 
        }

        void Diver(string currentDir, string pattern, Action<string> action)
        {
            if (Directory.Exists(currentDir))
            {
                string[] subDirs = Directory.GetDirectories(currentDir);
                foreach (string dir in subDirs)
                {
                    Diver(dir, pattern, action);
                }
                string[] files = Directory.GetFiles(currentDir, pattern);
                foreach (string file in files)
                    action(file);
            }
        }
    }
}