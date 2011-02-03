using System.Collections.Generic;
using System.Linq;

namespace Frog.Domain
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
            return items.Select(s => new NUnitTask(s)).ToList();
        }
    }
}