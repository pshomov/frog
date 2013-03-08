using Frog.Domain.RepositoryTracker;
using Nancy;

namespace Frog.UI.Web2
{
    public class WebModule : NancyModule
    {
        public WebModule()
        {
            Get["/project/codebase/{company}/{project}/{repository}/task/{taskIndex}"] =
                request => ProjectActions.GetTaskTerminalOutput(
                    ProjectActions.GetCodebaseProjectUrl(request.company, request.project,
                                                         request.repository), request.taskIndex);
            Get["/project/codebase/{company}/{project}/{repository}/task"] =
                request => ProjectActions.GetAllTaskTerminalOutput(
                    ProjectActions.GetCodebaseProjectUrl(request.company, request.project,
                                                         request.repository),
                    Request.Query.lastOutputChunkIndex, Request.Query.taskIndex);
            Get["/project/codebase/{company}/{project}/{repository}/force"] = request =>
                {
                    ProjectActions.ForceBuild(ProjectActions.GetCodebaseProjectUrl(request.company, request.project,
                                                                                   request.repository));

                    return HttpStatusCode.OK;
                };
            Get["/project/codebase/{company}/{project}/{repository}/history"] =
                request => ProjectActions.GetProjectHistory(ProjectActions.GetCodebaseProjectUrl(request.company,
                                                                                                 request.project,
                                                                                                 request.repository));
            Get["/project/codebase/{company}/{project}/{repository}/data"] =
                request => ProjectActions.GetProjectStatus(ProjectActions.GetCodebaseProjectUrl(request.company,
                                                                                                request.project,
                                                                                                request.repository));

            Get["/project/codebase/{company}/{project}/{repository}/status"] =
                request => View["status.html"];

            Post["/project/register"] = request =>
                {
                    ServiceLocator.Bus.Send(new RegisterRepository {Repo = request.url});
                    return Response.AsRedirect("/");
                };
        }
    }
}