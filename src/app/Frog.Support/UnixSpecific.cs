using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Frog.Support
{
    class UnixSpecific
    {
        public static string UnixTotalProcessorTime(int processId)
        {
            var p = new ProcessWrapper("python",string.Format("{0} {1}", Path.Combine(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "support_scripts"), "cpu_times.py"), processId));
            var processStrings = new List<String>();
            p.OnStdOutput += s => { if (!s.IsNullOrEmpty()) processStrings.Add(s); };
            p.Execute();
            p.WaitForProcess(10000);
            p.Dispose();
            return ParsePSInfo(processStrings);
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

        internal static string ParsePSInfo(List<string> processStrings)
        {
            if (processStrings.Count == 1) return processStrings[0];
            throw new InvalidOperationException("Process could not be found. Output from CPU sampler is:"+string.Join(",", processStrings.ToArray()));
        }
    }
}
