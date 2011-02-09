using System;
using NUnit.Framework;

namespace Frog.Domain.Specs
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
            catch (Exception e)
            {
                GivenCleanup();
                throw;
            }
            try
            {
                When();
            }
            catch (Exception e)
            {
                GivenCleanup();
                WhenCleanup();
                throw;
            }
        }

        public virtual void SetupDependencies(){}
        public abstract void Given();
        public abstract void When();

        protected virtual void GivenCleanup(){}
        protected virtual void WhenCleanup(){}
        [TearDown]
        void CaseCleanup() {GivenCleanup(); WhenCleanup();}
    }
}