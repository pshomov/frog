using System.IO;
using Frog.Support;

namespace Frog.Domain.Integration
{
    public class SubfolderWorkingAreaGoverner : WorkingAreaGoverner
    {
        readonly string allocationRoot;

        public SubfolderWorkingAreaGoverner(string allocationRoot)
        {
            this.allocationRoot = allocationRoot;
        }

        public string AllocateWorkingArea()
        {
            var allocateWorkingAreaPath = Path.Combine(allocationRoot, Path.GetRandomFileName());
            Directory.CreateDirectory(allocateWorkingAreaPath);
            return allocateWorkingAreaPath;
        }

        public void DeallocateWorkingArea(string allocatedArea)
        {
            OSHelpers.NukeDirectory(allocatedArea);
        }
    }
}