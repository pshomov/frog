using System.IO;
using NUnit.Framework;

namespace Frog.Domain.Specs
{
    [TestFixture]
    public class WorkingAreaSpecs : BDD
    {
        WorkingArea workingArea;
        string allocationRoot;
        string allocatedArea;

        public override void Given()
        {
            allocationRoot = Path.Combine(GitTestSupport.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(allocationRoot);
            workingArea = new SubfolderWorkingArea(allocationRoot);
        }

        public override void When()
        {
            allocatedArea = workingArea.AllocateWorkingArea();
        }

        [Test]
        public void should_allocate_working_areas_under_the_root_it_has_been_assigned()
        {
            Assert.That(allocatedArea.StartsWith(allocationRoot) && allocatedArea.Length > allocationRoot.Length);
        }

        [Test]
        public void should_create_a_folder_with_specified_name()
        {
            Assert.That(Directory.Exists(allocatedArea));
        }

        [Test]
        public void should_have_the_allocated_area_empty()
        {
            Assert.That(Directory.GetFiles(allocatedArea).Length == 0 &&
                        Directory.GetDirectories(allocatedArea).Length == 0);
        }
    }
}