using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Frog.Support
{
    class UnixSpecific
    {
        public static TimeSpan UnixTotalProcessorTime(int processId)
        {
            var p = new ProcessWrapper("ps", "-o pid=,ppid=,time=");
            var processStrings = new List<String>();
            p.OnStdOutput += s => { if (!s.IsNullOrEmpty()) processStrings.Add(s); };
            p.Execute();
            p.WaitForProcess();
            var processes = ParsePSInfo(processStrings);
            var cputime = GetTotalCpuTime(processes, processId);
            return cputime;
        }

        private static TimeSpan GetTotalCpuTime(List<long[]> processes, int pid)
        {
            var cputime = GetCpuTimeFor(processes, pid);
            cputime += GetChildrenCpuTimeFor(processes, pid);
            return cputime;
        }

        internal static TimeSpan GetChildrenCpuTimeFor(List<long[]> processes, int pid)
        {
            return TimeSpan.FromTicks(processes.Where(longs => longs[1] == pid).Sum(longs => GetTotalCpuTime(processes, (int) longs[0]).Ticks));
        }

        internal static TimeSpan GetCpuTimeFor(IEnumerable<long[]> processes, int pid)
        {
            return TimeSpan.FromTicks(processes.Single(ints => ints[0] == pid)[2]);
        }

        internal static List<long[]> ParsePSInfo(List<string> processStrings)
        {
            var processes = new List<long[]>();
            processStrings.ForEach(s =>
                                       {
                                           var strings = s.Split(' ');
                                           var pid = Int32.Parse(strings[0]);
                                           var ppid = Int32.Parse(strings[1]);
                                           var cpu = TimeSpan.Parse(strings[2]).Ticks;
                                           processes.Add(new[]{pid, ppid, cpu});
                                       });
            return processes;
        }
    }
}
