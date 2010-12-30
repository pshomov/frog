using System.IO;

namespace Frog.Domain.Specs
{
    public class SubfolderWorkingArea : WorkingArea
    {
        readonly string allocationRoot;

        public SubfolderWorkingArea(string allocationRoot)
        {
            this.allocationRoot = allocationRoot;
        }

        public string AllocateWorkingArea()
        {
            var allocateWorkingAreaPath = Path.Combine(allocationRoot, Path.GetRandomFileName());
            Directory.CreateDirectory(allocateWorkingAreaPath);
            return allocateWorkingAreaPath;
        }
    }
}