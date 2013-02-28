using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using SaaS.Engine;

namespace Frog.Domain.Specs
{
    [TestFixture]
    public class ProjectIdentityMatching
    {
        [Test]
        public void should_match_http_project_with_no_username_password()
        {
            var id1 = new ProjectId("http://fle.com/dught.git");
            var id2 = new ProjectId("http://fle.com/dught.git");
            Assert.That(id1, Is.EqualTo(id2));
        }

        [Test]
        public void should_match_http_project_with_username_password()
        {
            var id1 = new ProjectId("http://petar:pass!!333@fle.com/dught.git");
            var id2 = new ProjectId("http://fle.com/dught.git");
            Assert.That(id1, Is.EqualTo(id2));
        }

        [Test]
        public void should_match_https_project_with_no_username_password()
        {
            var id1 = new ProjectId("https://fle.com/dught.git");
            var id2 = new ProjectId("https://fle.com/dught.git");
            Assert.That(id1, Is.EqualTo(id2));
        }

        [Test]
        public void should_match_https_project_with_username_password()
        {
            var id1 = new ProjectId("https://petar:pass@fle.com/dught.git");
            var id2 = new ProjectId("https://fle.com/dught.git");
            Assert.That(id1, Is.EqualTo(id2));
        }
    }
}
