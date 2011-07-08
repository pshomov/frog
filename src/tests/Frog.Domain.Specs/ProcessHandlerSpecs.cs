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
    }
}