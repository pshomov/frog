using System.Collections.Generic;
using System.IO;

namespace Frog.Domain.Integration.TaskSources.BuildSystems.Custom
{
    public class CustomFileFinder : TaskFileFinder
    {
        readonly PathFinder pathFinder;

        public CustomFileFinder(PathFinder pathFinder)
        {
            this.pathFinder = pathFinder;
        }

        public List<string> FindFiles(string baseFolder)
        {
            List<string> rakeFile = new List<string>();
            var baseFolderFull = Path.GetFullPath(baseFolder);
            pathFinder.FindFilesAtTheBase(s => rakeFile.Add(s.Remove(0, baseFolderFull.Length + 1)), "runz.me",
                                          baseFolder);
            return rakeFile;
        }
    }
}