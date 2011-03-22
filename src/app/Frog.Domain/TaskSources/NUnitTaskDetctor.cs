using System.Collections.Generic;
using System.IO;
using System.Linq;
using Frog.Domain.CustomTasks;
using Frog.Support;

namespace Frog.Domain.TaskSources
{
    public class NUnitTaskDetctor : TaskSource
    {
        readonly FileFinder projectFileRepo;

        public NUnitTaskDetctor(FileFinder projectFileRepo)
        {
            this.projectFileRepo = projectFileRepo;
        }

        public IList<ITask> Detect(string projectFolder)
        {
            var items = projectFileRepo.FindAllNUnitAssemblies(projectFolder);
            return items.Select(s => (ITask) new NUnitTask(ProjectPathToAssemblyPath(s))).ToList();
        }

        string ProjectPathToAssemblyPath(string projectPath)
        {
            var projectName = Path.GetFileNameWithoutExtension(projectPath);
            var assemblyPojectFolder = Path.GetDirectoryName(projectPath);
            return Path.Combine(Path.Combine(assemblyPojectFolder, Os.DirChars("bin/Debug")), projectName + ".dll");
        }
    }
}