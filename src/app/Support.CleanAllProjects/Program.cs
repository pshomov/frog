using System.Linq;
using Frog.Domain.Integration;
using Frog.Support;

namespace Support.CleanAllProjects
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var riak = new RiakProjectRepository(
                OSHelpers.RiakHost(),
                OSHelpers.RiakPort(),
                "projects");
            riak.AllProjects.ToList().ForEach(document => riak.RemoveProject(document.projecturl));
        }
    }
}