using System;
using System.Collections.Generic;
using Frog.Domain.Specs.PathGoble;

namespace Frog.Domain
{
    public interface FileFinder
    {
        IList<string> FindAllNUnitAssemblies();
    }

    public class DefaultFileFinder : FileFinder
    {
        readonly PathFinder pathFinder;

        public DefaultFileFinder(PathFinder pathFinder)
        {
            this.pathFinder = pathFinder;
        }

        public IList<string> FindAllNUnitAssemblies()
        {
            var dlls = new List<string>();
            pathFinder.apply(dlls.Add, "*.TEST.DLL");
            return dlls;
        }
    }
}