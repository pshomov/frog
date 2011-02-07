using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Frog.Support;

namespace Frog.Domain.CustomTasks
{
    public class NUnitTaskDetctor
    {
        readonly FileFinder projectFileRepo;

        public NUnitTaskDetctor(FileFinder projectFileRepo)
        {
            this.projectFileRepo = projectFileRepo;
        }

        public IList<NUnitTask> Detect()
        {
            var items =  projectFileRepo.FindAllNUnitAssemblies();
            return items.Select(s => new NUnitTask(ProjectPathToAssemblyPath(s))).ToList();
        }

        private string ProjectPathToAssemblyPath(string projectPath)
        {
            var projectName = Path.GetFileNameWithoutExtension(projectPath);
            var assemblyPojectFolder = Path.GetDirectoryName(projectPath);
            return Path.Combine(Path.Combine(assemblyPojectFolder, Os.DirChars("bin/Debug")), projectName+".dll");
        }
    }
}