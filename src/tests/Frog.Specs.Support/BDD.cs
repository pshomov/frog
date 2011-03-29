using System;
using NUnit.Framework;

namespace Frog.Specs.Support
{
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
                GivenCleanup();
                throw;
            }
            try
            {
                When();
            }
            catch (Exception)
            {
                GivenCleanup();
                WhenCleanup();
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