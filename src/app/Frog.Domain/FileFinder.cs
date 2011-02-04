using System;
using System.Collections.Generic;

namespace Frog.Domain
{
    public interface FileFinder
    {
        List<string> FindAllNUnitAssemblies();
    }

    public class DefaultFileFinder : FileFinder
    {
        readonly PathFinder pathFinder;

        public DefaultFileFinder(PathFinder pathFinder)
        {
            this.pathFinder = pathFinder;
        }

        public List<string> FindAllNUnitAssemblies()
        {
            var dlls = new List<string>();
            pathFinder.apply(dlls.Add, "*.TEST.DLL");
            return dlls;
        }
    }
}