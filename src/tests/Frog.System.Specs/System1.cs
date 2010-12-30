using Frog.Domain;
using Frog.Domain.Specs;
using NUnit.Framework;

namespace Frog.System.Specs
{
    [TestFixture]
    public class System1 : BDD
    {
        Valve valve;
        GitDriver driver;
        PipelineOfTasks pipeline;
        SubfolderWorkingArea area;

        public override void Given()
        {
            driver = new GitDriver("c:\\test_repos", "test", "http://github.com/pshomov/xray.git");
            pipeline = new PipelineOfTasks(new ExecTask(@"C:\Windows\Microsoft.NET\Framework\v3.5\msbuild.exe xray.sln"));
            area = new SubfolderWorkingArea("c:\\working_area");
            valve = new Valve(driver, pipeline, area);
        }

        public override void When()
        {
            valve.Check();
        }

        [Test]
        public void should_checkout_and_build_project()
        {
            Assert.That(false);
        }

    }
}