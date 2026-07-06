using Mediator;
using Workplan.SharedKernel.Common;

namespace Workplan.Application.Features.Projects.Queries.GetProjectById;

public sealed record GetProjectByIdQuery(Guid Id) : IRequest<Result<ProjectDto>>;
