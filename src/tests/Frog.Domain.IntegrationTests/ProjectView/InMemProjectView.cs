using NUnit.Framework;

namespace Frog.Domain.IntegrationTests.ProjectView
{
    [TestFixture]
    public class InMemProjectView : ProjectViewSpecs
    {
        protected override Integration.UI.ProjectView GetProjectView()
        {
            return new Integration.UI.InMemProjectView();
        }
    }
}