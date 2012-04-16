﻿using System.Collections.Generic;
using System.IO;
using System.Linq;
using Frog.Domain.TaskSources;
using Frog.Support;

namespace Frog.Domain.BuildSystems.Solution
{
    public class NUnitTaskDetector : TaskSource
    {
        public NUnitTaskDetector(TaskFileFinder _projectTaskFileRepo)
        {
            this._projectTaskFileRepo = _projectTaskFileRepo;
        }

        public IList<Task> Detect(string projectFolder)
        {
            var items = _projectTaskFileRepo.FindFiles(projectFolder);
            return items.Select(s => (Task) new NUnitTask(ProjectPathToAssemblyPath(s))).ToList();
        }

        readonly TaskFileFinder _projectTaskFileRepo;

        string ProjectPathToAssemblyPath(string projectPath)
        {
            var projectName = Path.GetFileNameWithoutExtension(projectPath);
            var assemblyPojectFolder = Path.GetDirectoryName(projectPath);
            return Path.Combine(Path.Combine(assemblyPojectFolder, Os.DirChars("bin/Debug")), projectName + ".dll");
        }
    }
}