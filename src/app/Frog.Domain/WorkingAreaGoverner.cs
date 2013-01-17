namespace Frog.Domain
{
    public interface WorkingAreaGoverner
    {
        string AllocateWorkingArea();
        void DeallocateWorkingArea(string allocatedArea);
    }
}