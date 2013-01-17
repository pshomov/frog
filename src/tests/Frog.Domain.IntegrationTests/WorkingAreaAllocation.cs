using System.IO;
using Frog.Domain.Integration;
using Frog.Specs.Support;
using NUnit.Framework;

namespace Frog.Domain.IntegrationTests
{
    [TestFixture]
    public class WorkingAreaAllocation : BDD
    {
        WorkingAreaGoverner workingAreaGoverner;
        string allocationRoot;
        string allocatedArea;

        protected override void Given()
        {
            allocationRoot = Path.Combine(GitTestSupport.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(allocationRoot);
            workingAreaGoverner = new SubfolderWorkingAreaGoverner(allocationRoot);
        }

        protected override void When()
        {
            allocatedArea = workingAreaGoverner.AllocateWorkingArea();
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

    [TestFixture]
    public class WorkingAreaDeAllocation : BDD
    {
        WorkingAreaGoverner workingAreaGoverner;
        string allocationRoot;
        string allocatedArea;

        protected override void Given()
        {
            allocationRoot = Path.Combine(GitTestSupport.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(allocationRoot);
            workingAreaGoverner = new SubfolderWorkingAreaGoverner(allocationRoot);
            allocatedArea = workingAreaGoverner.AllocateWorkingArea();
        }

        protected override void When()
        {
            workingAreaGoverner.DeallocateWorkingArea(allocatedArea);
        }

        [Test]
        public void should_have_deleted_the_folder()
        {
            Assert.That(!Directory.Exists(allocatedArea));
        }

    }
}