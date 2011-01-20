using System.Text;
using Frog.Support;
using NUnit.Framework;

namespace Frog.Domain.Specs
{
    [TestFixture]
    public class ProcessHandlerSpecs : BDD
    {
        StringBuilder err;
        ProcessWrapper pw;

        public override void Given()
        {
            err = new StringBuilder();
            pw = new ProcessWrapper("cmd.exe", "/c asdfasdfddd@!#!11fasfasdf.asdfasdf");
            pw.OnErrorOutput += output => err.Append(output);
        }

        public override void When()
        {
            pw.Execute();
            pw.WaitForProcess();
        }

        [Test]
        public void should_capture_std_error_output()
        {
            Assert.That(err.ToString(), Is.Not.Empty);
        }
    }

    [TestFixture]
    public class ProcessHandlerStandardOutCaptureSpecs : BDD
    {
        StringBuilder std;
        ProcessWrapper pw;

        public override void Given()
        {
            std = new StringBuilder();
            pw = new ProcessWrapper("cmd.exe", "/c dir");
            pw.OnStdOutput += output => std.Append(output);
        }

        public override void When()
        {
            pw.Execute();
            pw.WaitForProcess();
        }

        [Test]
        public void should_capture_std_output()
        {
            Assert.That(std.ToString(), Is.Not.Empty);
        }
    }
}