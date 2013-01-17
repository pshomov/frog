namespace Frog.Domain.BuildSystems.Solution
{
    public class NUnitTask : Task
    {
        public string Assembly
        {
            get { return assembly; }
        }

        public NUnitTask(string assembly)
        {
            this.assembly = assembly;
        }

        readonly string assembly;
    }
}