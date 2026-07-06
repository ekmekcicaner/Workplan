using Mediator;
using Workplan.SharedKernel.Common;

namespace Workplan.Application.Features.Projects.Queries.GetProjects;

public sealed record GetProjectsQuery(bool IncludeInactive = false) : IRequest<Result<List<ProjectDto>>>;
