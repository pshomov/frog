using System;
using System.Collections.Generic;
using Frog.Domain.UI;

namespace Frog.Domain.Integration
{
    public class PersistentProjectView : ProjectView
    {
        public BuildStatus GetBuildStatus(Guid id)
        {
            throw new NotImplementedException();
        }

        public Guid GetCurrentBuild(string repoUrl)
        {
            throw new NotImplementedException();
        }

        public void SetCurrentBuild(string repoUrl, Guid buildId, string comment, string revision)
        {
            throw new NotImplementedException();
        }

        public bool ProjectRegistered(string projectUrl)
        {
            throw new NotImplementedException();
        }

        public List<BuildHistoryItem> GetListOfBuilds(string repoUrl)
        {
            throw new NotImplementedException();
        }
    }
}