using NUnit.Framework;

namespace Frog.Domain.IntegrationTests.ProjectView
{
    [TestFixture]
    public class InMemProjectView : ProjectViewSpecs
    {
        protected override UI.ProjectView GetProjectView()
        {
            return new UI.InMemProjectView();
        }
    }
}