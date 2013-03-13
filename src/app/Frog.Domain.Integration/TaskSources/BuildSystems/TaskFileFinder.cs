using System.Collections.Generic;

namespace Frog.Domain.Integration.TaskSources.BuildSystems
{
    public interface TaskFileFinder
    {
        List<string> FindFiles(string baseFolder);
    }
}