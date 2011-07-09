using System;
using System.ComponentModel;
using System.Text;
using Frog.Support;
using NUnit.Framework;

namespace Frog.Domain.Specs
{

    [TestFixture]
    public class ProcessHandlerIOCaptureSpecs
    {

        [Test]
        public void should_capture_std_output()
        {
            var std = new StringBuilder();
            var pw = new ProcessWrapper("ruby", "-e 'puts \"ff\"'");
            pw.OnStdOutput += output => std.Append(output);

            pw.Execute();
            pw.WaitForProcess();

            Assert.That(std.ToString(), Is.Not.Empty);
        }

        [Test]
        public void should_capture_std_error_output()
        {
            var err = new StringBuilder();
            var pw = new ProcessWrapper("ruby", "-e '$stderr.puts(\"fle\")'");
            pw.OnErrorOutput += output => err.Append(output);
            pw.Execute();
            pw.WaitForProcess();
            Assert.That(err.ToString(), Is.Not.Empty);
        }

        [Test]
        public void should_run_when_no_std_error_output_is_captured()
        {
            var pw = new ProcessWrapper("ruby", "-e '$stderr.puts(\"fle\")'");
            pw.Execute();
            pw.WaitForProcess();
        }

        [Test]
        public void should_throw_exception_when_executable_not_found()
        {
            var pw = new ProcessWrapper("rubyyyyyy", "-e '$stderr.puts(\"fle\")'");
            try
            {
                pw.Execute();
                pw.WaitForProcess();
                Assert.Fail("Should have thrown an exception");
            }
            catch (ApplicationNotFoundException e)
            {
                
            }
        }

        [Test]
        public void should_indicate_cpu_usage_when_process_consumes_one()
        {
            var pw = new ProcessWrapper("ruby", "-e '100000000.times {|e| e}'");
            pw.Execute();
            pw.WaitForProcess(1);
            var tpt = pw.TotalProcessorTime;
            pw.WaitForProcess(100);
            var tpt1 = pw.TotalProcessorTime;
            pw.Kill();
			
            Assert.That(tpt, Is.LessThan(tpt1));
        }
        [Test]
        public void should_indicate_no_cpu_usage_when_process_does_nothing()
        {
            var pw = new ProcessWrapper("ruby", "-e 'sleep 30'");
            pw.Execute();
            pw.WaitForProcess(10);
            var tpt = pw.TotalProcessorTime;
            pw.WaitForProcess(100);
            var tpt1 = pw.TotalProcessorTime;
            pw.Kill();
			
            Assert.That(tpt, Is.EqualTo(tpt1));
        }
    }
}