namespace Frog.Domain.CustomTasks
{
    public class NUnitTask : ITask
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