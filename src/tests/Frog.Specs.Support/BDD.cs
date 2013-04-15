using System;
using NUnit.Framework;

namespace Frog.Specs.Support
{
    [TestFixture]
    public abstract class BDD
    {
        [SetUp]
        public void Setup()
        {
            SetupDependencies();
            try
            {
                Given();
            }
            catch (Exception)
            {
                try
                {
                    GivenCleanup();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
                throw;
            }
            try
            {
                When();
            }
            catch (Exception)
            {
                try
                {
                    GivenCleanup();
                    WhenCleanup();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
                throw;
            }
        }

        protected virtual void SetupDependencies(){}
        protected abstract void Given();
        protected abstract void When();

        protected virtual void GivenCleanup(){}
        protected virtual void WhenCleanup(){}
        [TearDown]
        public void CaseCleanup() {GivenCleanup(); WhenCleanup();}
    }
}