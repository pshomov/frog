using System;
using Frog.WiredUp;

namespace Frog.Agent
{
    public static class Program
    {
        static void Main(string[] args)
        {
            using (var agent = new AgentDeploumentWireUp())
            {
                agent.Start(Guid.NewGuid());
                Console.ReadLine();
            }
        }

    }
}
