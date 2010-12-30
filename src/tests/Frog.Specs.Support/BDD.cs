using NUnit.Framework;

namespace Frog.Domain.Specs
{
    public abstract class BDD
    {
        [SetUp]
        public void Setup()
        {
            Given();
            When();
        }

        public abstract void Given();
        public abstract void When();
    }
}