using System;
using SimpleCQRS;

namespace Frog.Domain.RepositoryTracker
{
    public class Build : Command
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