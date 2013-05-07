using System;
using System.Linq;
using System.Text;
using Frog.Domain;
using Lokad.Cqrs.AtomicStorage;
using SaaS.Client.Projections.Frog.Projects;
using SaaS.Engine;
using Build = Frog.Domain.Build;
using TaskInfo = SaaS.Engine.TaskInfo;

namespace Frog.UI.Web2
{
    public class ProjectActions
    {
        public static void ForceBuild(string projectUrl)
        {
            ProjectBuild lastBuild =
                ServiceLocator.Store.GetReader<ProjectId, ProjectHistory>()
                              .Load(new ProjectId(projectUrl))
                              .CurrentHistory;
            ServiceLocator.Bus.Send(new BuildRequest
                {
                    Id = Guid.NewGuid(),
                    RepoUrl = lastBuild.ProjectUrl,
                    Revision = new RevisionInfo {Revision = lastBuild.RevisionNr},
                    CapabilitiesNeeded = new string[]{},
                });
        }

        public static object GetProjectHistory(string projectUrl)
        {
            IDocumentReader<ProjectId, ProjectHistory> documentReader =
                ServiceLocator.Store.GetReader<ProjectId, ProjectHistory>();
            ProjectHistory projectHistory;
            if (documentReader.TryGet(new ProjectId(projectUrl), out projectHistory))
            {
                return (
                           new
                               {
                                   items =
                                       projectHistory.Items
                               });
            }
            return null;
        }

        public static object GetAllTaskTerminalOutput(string projectUrl, int lastChunkIndex, int taskIndex)
        {
            var documentReader =
                ServiceLocator.Store.GetReader<ProjectId, ProjectHistory>();
            ProjectHistory projectHistory;
            if (documentReader.TryGet(new ProjectId(projectUrl), out projectHistory))
            {
                var tasks = projectHistory.Current.Tasks;
                var activeTask = taskIndex;
                var content = new StringBuilder();
                for (var i = taskIndex; i < tasks.Count; i++)
                {
                    var sinceIndex = i == taskIndex ? lastChunkIndex : 0;
                    var terminalId = projectHistory.Current.Tasks[i].Id;
                    var terminalOutput = projectHistory.Current.TerminalOutput[terminalId];
                    var terminalOutputR =
                        terminalOutput.Where((s, i1) => i1 > sinceIndex);
                    if (terminalOutput.Count <= sinceIndex) continue;
                    activeTask = i;
                    lastChunkIndex = terminalOutput.Count;
                    content.Append(terminalOutputR.DefaultIfEmpty().Aggregate((s, s1) => s + s1 + "\r\n"));
                }
                return
                    new
                        {
                            terminalOutput = content.ToString(),
                            activeTask = activeTask,
                            lastChunkIndex = lastChunkIndex
                        };
            }
            return null;
        }

        public static string GetGithubProjectUrl(string user, string project)
        {
            return String.Format("http://github.com/{0}/{1}", user, project);
        }

        public static string GetCodebaseProjectUrl(string company, string project, string repository)
        {
            return String.Format("http://{0}.codebasehq.com/{1}/{2}.git", company, project, repository);
        }

        public static object GetProjectStatus(string projectUrl)
        {
            IDocumentReader<ProjectId, ProjectHistory> documentReader =
                ServiceLocator.Store.GetReader<ProjectId, ProjectHistory>();
            ProjectHistory projectHistory;
            if (documentReader.TryGet(new ProjectId(projectUrl), out projectHistory))
            {
                return new {status = projectHistory.Current.Status, tasks = projectHistory.CurrentTasks};
            }
            return null;
        }

        public static object GetTaskTerminalOutput(string projectUrl, int taskIndex)
        {
            IDocumentReader<ProjectId, ProjectHistory> documentReader =
                ServiceLocator.Store.GetReader<ProjectId, ProjectHistory>();
            ProjectHistory projectHistory;
            if (documentReader.TryGet(new ProjectId(projectUrl), out projectHistory))
            {
                TaskInfo taskInfo = projectHistory.Current.Tasks[taskIndex];
                return
                    new
                        {
                            terminalOutput = projectHistory.Current.TerminalOutput[taskInfo.Id]
                        };
            }
            return null;
        }
    }
}