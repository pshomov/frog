using System;
using System.Diagnostics;
using System.IO;
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
        public void should_allocate_working_areas_under_the_root_it_has_been_assigned()
        {
            Assert.That(err.ToString(), Is.Not.Empty);
        }

    }

    public class ProcessWrapper
    {
        readonly string cmdExe;
        readonly string arguments;
        Process process;
        public event OutputCallback OnErrorOutput; 
        public ProcessWrapper(string cmdExe, string arguments)
        {
            this.cmdExe = cmdExe;
            this.arguments = arguments;
        }

        public void Execute()
        {
            process = ProcessHelpers.Start(cmdExe, arguments);
            process.ErrorDataReceived += (sender, args) => OnErrorOutput(args.Data);
            process.BeginErrorReadLine();
        }

        public void WaitForProcess()
        {
            process.WaitForExit();
        }
    }

    public delegate void OutputCallback(string output);
}