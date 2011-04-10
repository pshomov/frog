using System.Collections.Generic;
using System.IO;
using System.Linq;
using Frog.Domain.CustomTasks;
using Frog.Domain.TaskSources;
using Frog.Support;

namespace Frog.Domain.BuildSystems.Solution
{
    public class NUnitTaskDetector : TaskSource
    {
        readonly TaskFileFinder _projectTaskFileRepo;

        public NUnitTaskDetector(TaskFileFinder _projectTaskFileRepo)
        {
            this._projectTaskFileRepo = _projectTaskFileRepo;
        }

        public IList<ITask> Detect(string projectFolder)
        {
            var items = _projectTaskFileRepo.FindFiles(projectFolder);
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