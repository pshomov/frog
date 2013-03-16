using System;
using System.Diagnostics;
using System.Text;
using Frog.Support;
using NUnit.Framework;

namespace Frog.Domain.IntegrationTests
{
    [TestFixture]
    public class ProcessHandlerSpec
    {

        [Test]
        public void should_capture_std_output()
        {
            var std = new StringBuilder();
            var pw = new ProcessWrapper("ruby", "-e 'puts \"ff\"'");
            pw.OnStdOutput += output => std.Append(output);

            pw.Execute();
            pw.WaitForProcess(5000);
            pw.Dispose();

            Assert.That(std.ToString(), Is.Not.Empty);
        }

        [Test]
        public void should_capture_std_error_output()
        {
            var err = new StringBuilder();
            var pw = new ProcessWrapper("ruby", "-e '$stderr.puts(\"fle\")'");
            pw.OnErrorOutput += output => err.Append(output);
            pw.Execute();
            pw.WaitForProcess(5000);
            pw.Dispose();
            Assert.That(err.ToString(), Is.Not.Empty);
        }

        [Test]
        public void should_run_when_no_std_error_output_is_captured()
        {
            var pw = new ProcessWrapper("ruby", "-e '$stderr.puts(\"fle\")'");
            pw.Execute();
            pw.WaitForProcess(5000);
        }

        [Test]
        public void should_throw_exception_when_executable_not_found()
        {
            var pw = new ProcessWrapper("rubyyyyyy", "-e '$stderr.puts(\"fle\")'");
            try
            {
                pw.Execute();
                Assert.Fail("Should have thrown an exception");
            }
            catch (ApplicationNotFoundException)
            {
            }
        }

        [Test]
        public void should_indicate_cpu_usage_when_process_consumes_one()
        {
            var pw = new ProcessWrapper("ruby", "-e '100000000.times {|e| e}'");
            pw.Execute();
            pw.WaitForProcess(1);
            var tpt = pw.ProcessTreeCPUUsageId;
            pw.WaitForProcess(500);
            var tpt1 = pw.ProcessTreeCPUUsageId;
            pw.Dispose();
			
            Assert.That(tpt, Is.Not.EqualTo(tpt1));
        }

        [Test]
        public void should_indicate_no_cpu_usage_when_process_does_nothing()
        {
            var pw = new ProcessWrapper("ruby", "-e 'sleep 30'");
            pw.Execute();
            pw.WaitForProcess(1000);
            var tpt = pw.ProcessTreeCPUUsageId;
            pw.WaitForProcess(2000);
            var tpt1 = pw.ProcessTreeCPUUsageId;
            pw.Dispose();
			
            Assert.That(tpt, Is.EqualTo(tpt1));
        }

        [Test]
        public void should_make_the_process_nonexistent_after_killing_it()
        {
            var pw = new ProcessWrapper("ruby", @"-e ""system(\""ruby -e 'sleep 300'\"")""");
            pw.Execute();
            var processId = pw.Id;
            pw.Dispose();
            Assert.That(ProcessHasExited(processId));
        }

        [Test]
        public void should_throw_an_exception_when_trying_to_get_process_snapshot_for_process_that_has_died()
        {
            var pw = new ProcessWrapper("ruby", @"-e exit 0");
            pw.Execute();
            pw.WaitForProcess(1000);
            var processId = pw.Id;
//            pw.Dispose();
            Assert.That(ProcessHasExited(processId));
            try
            {
                var tpt = pw.ProcessTreeCPUUsageId;
                Assert.Fail("the process should have been gone, operation should have failed");
            }
            catch(InvalidOperationException)
            {
                // that's what we are aiming for ;)
            }
            pw.Dispose();
        }

        private bool ProcessHasExited(int processWrapper)
        {
            try
            {
                Process.GetProcessById(processWrapper);
                return false;
            }
            catch (ArgumentException)
            {
                return true;
            }
        }
    }
}