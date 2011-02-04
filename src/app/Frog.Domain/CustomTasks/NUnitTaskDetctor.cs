using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Frog.Domain.CustomTasks
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
            RemoveAllDuplicatingAssemblies(items);
            return items.Select(s => new NUnitTask(s)).ToList();
        }

        void RemoveAllDuplicatingAssemblies(List<string> items)
        {
            var fileNames = items.Select(s1 => Path.GetFileName(s1).ToUpperInvariant());
            var distinctFileNames = fileNames.Distinct();
            var duplicates = fileNames.ToList();
            distinctFileNames.ToList().ForEach(s4 => duplicates.Remove(s4));
            duplicates = duplicates.Distinct().ToList();
            items.RemoveAll(s2 => duplicates.Contains(Path.GetFileName(s2).ToUpperInvariant()));
        }
    }
}