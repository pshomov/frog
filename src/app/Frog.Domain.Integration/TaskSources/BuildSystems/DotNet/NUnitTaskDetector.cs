using System.Collections.Generic;
using System.IO;
using System.Linq;
using Frog.Support;

namespace Frog.Domain.Integration.TaskSources.BuildSystems.DotNet
{
    public class NUnitTaskDetector : TaskSource
    {
        public NUnitTaskDetector(TaskFileFinder _projectTaskFileRepo)
        {
            this._projectTaskFileRepo = _projectTaskFileRepo;
        }

        public IEnumerable<TaskDescription> Detect(string projectFolder, out bool shouldStop)
        {
            shouldStop = false;
            var items = _projectTaskFileRepo.FindFiles(projectFolder);
            return items.Select(s => new ShellTaskDescription() {Command = "nunit", Arguments = ProjectPathToAssemblyPath(s)});
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