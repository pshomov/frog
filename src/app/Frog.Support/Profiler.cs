using System;
using System.IO;

namespace Frog.Support
{
    public static class Profiler
    {
        public interface LoggingBridge
        {
            void TimePeriod(DateTime start, DateTime end, string msg);
        }

        public class LogFileLoggingBridge : LoggingBridge
        {
            private string filename;

            public LogFileLoggingBridge()
            {
                filename = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                                        "runz_measurements.log");
            }

            public void TimePeriod(DateTime start, DateTime end, string msg)
            {
                File.AppendAllText(filename, string.Format("Scope {0} started at {1} and finished at {2}", msg, start, end));
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