using System;
using System.IO;

namespace Frog.Support
{
    public static class Profiler
    {
        public interface LoggingBridge : IDisposable
        {
            void TimePeriod(DateTime start, DateTime end, string msg);
        }

        public class LogFileLoggingBridge : LoggingBridge
        {
            private string filename;
            private StreamWriter fileStream;

            public LogFileLoggingBridge(string systemTestsLog)
            {
                filename = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                                        systemTestsLog);
                fileStream = new StreamWriter(filename, true);
            }

            public void TimePeriod(DateTime start, DateTime end, string msg)
            {
                fileStream.WriteLine(string.Format("Scope \"{0}\" started at {1} and finished at {2}, so it took {3} ms\r\n", msg, start.ToString("o"), end.ToString("o"), (end-start).TotalMilliseconds));
            }

            public void Dispose()
            {
                fileStream.Dispose();
            }
        }

        private class MeasurementScope : IDisposable
        {
            private readonly string message;
            private readonly DateTime start;

            public MeasurementScope(string message)
            {
                this.message = message;
                start = DateTime.Now;
            }

            public void Dispose()
            {
                if (MeasurementsBridge != null)
                MeasurementsBridge.TimePeriod(start, DateTime.Now, message);
            }
        } 
        public static LoggingBridge MeasurementsBridge;

        public static IDisposable measure(string message)
        {
            return new MeasurementScope(message);
        }
    }
}