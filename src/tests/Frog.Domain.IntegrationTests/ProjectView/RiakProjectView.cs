using Frog.Domain.Integration;
using Frog.Support;
using NUnit.Framework;

namespace Frog.Domain.IntegrationTests.ProjectView
{
    [TestFixture]
    public class RiakProjectView : ProjectViewSpecs
    {
        protected override Integration.UI.ProjectView GetProjectView()
        {
            return new RiakBasedProjectView(OSHelpers.RiakHost(), OSHelpers.RiakPort(), "buildIds_test1", "repoIds_test");
        }
    }
}