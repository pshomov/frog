using System.Collections.Generic;

namespace Frog.Domain
{
    public interface FileFinder
    {
        IList<string> FindAllNUnitAssemblies();
    }
}