using Frog.Domain.Integration;
using Frog.Support;
using NUnit.Framework;

namespace Frog.Domain.IntegrationTests.ProjectView
{
    [TestFixture]
    public class RiakProjectView : ProjectViewSpecs
    {
        protected override UI.ProjectView GetProjectView()
        {
            return new PersistentProjectView(OSHelpers.RiakHost(), OSHelpers.RiakPort(), "buildIds_test1", "repoIds_test");
        }
    }
}