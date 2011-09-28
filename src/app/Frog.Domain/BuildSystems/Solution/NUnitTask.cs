namespace Frog.Domain.BuildSystems.Solution
{
    public class NUnitTask : Task
    {
        readonly string assembly;

        public NUnitTask(string assembly)
        {
            this.assembly = assembly;
        }

        public string Assembly
        {
            get { return assembly; }
        }
    }
}