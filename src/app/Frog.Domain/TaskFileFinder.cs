using System.Collections.Generic;

namespace Frog.Domain
{
    public interface TaskFileFinder
    {
        List<string> FindFiles(string baseFolder);
    }
}