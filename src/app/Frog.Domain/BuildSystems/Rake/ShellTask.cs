namespace Frog.Domain.BuildSystems.Rake
{
    public class ShellTask : Task
    {
        public string args;
        public string cmd = "";

        public override string ToString()
        {
            return "Shell task with command '{0}' and arguments '{1}'";
        }
    }
}