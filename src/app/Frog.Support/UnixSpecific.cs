using System;
using System.Collections.Generic;
using System.IO;

namespace Frog.Support
{
    class UnixSpecific
    {
        public static string UnixTotalProcessorTime(int processId)
        {
            var p = new ProcessWrapper("python",string.Format("{0} {1}", Path.Combine(Locations.SupportScriptsLocation, "cpu_times.py"), processId));
            var processStrings = new List<String>();
            var errorStrings = new List<String>();
            p.OnStdOutput += s => { if (!s.IsNullOrEmpty()) processStrings.Add(s); };
            p.OnErrorOutput += s => { if (!s.IsNullOrEmpty()) errorStrings.Add(s); };
            p.Execute();
            p.WaitForProcess(10000);
            p.Dispose();
            return ParseCPUTreeUsageInfo(processStrings, errorStrings);
        }

        internal static string ParseCPUTreeUsageInfo(List<string> processStrings, List<string> errorStrings)
        {
            if (processStrings.Count == 1) return processStrings[0];
            if (errorStrings.Count > 0 && errorStrings.Exists(s => s.Contains("NoSuchProcess"))) throw new InvalidOperationException("Process could not be found. Output from CPU sampler is:"+string.Join(",", processStrings.ToArray()));
            throw new ApplicationException(
                string.Format("Could not process cpu snapshot output. StdOutput: {0}, ErrOutput: {1}", string.Join(",", processStrings.ToArray()), string.Join(",", errorStrings.ToArray())));
        }
    }
}
