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
                agent.Start(Guid.Parse("38892791-8E48-4B95-90C3-B16CFA9BEFF8"));
                Console.ReadLine();
            }
        }

    }
}
