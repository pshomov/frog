using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Frog.Domain;

namespace Frog.Runner
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 0)
            {
                Console.WriteLine("Usage: Runz.Runner <srcdir>");
            }
            var pipeline = Frog.Agent.Program.GetPipeline();
            pipeline.OnBuildStarted += status => Console.WriteLine("Runz>> Build started {0}", status);
            pipeline.OnBuildUpdated += (i, guid, arg3) => Console.WriteLine("Runz>> Build update: task {0}, status is now {1}", i, arg3);
            pipeline.OnTerminalUpdate += info => Console.Write(info.Content);
            pipeline.OnBuildEnded += status => Console.WriteLine("Runz>> Build ended with status {0}", status);
            pipeline.Process(new SourceDrop(args[0]));
        }
    }
}
