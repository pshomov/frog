using System;

namespace Frog.Domain
{
    public class Build : SimpleCQRS.Command
    {
        public Guid Id { get; set; }
        public string RepoUrl { get; set; }
        public RevisionInfo Revision { get; set; }

        public Build()
        {
            Id = Guid.NewGuid();
        }
    }
}