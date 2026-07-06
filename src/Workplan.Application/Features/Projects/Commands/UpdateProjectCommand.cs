using Mediator;
using Workplan.SharedKernel.Common;

namespace Workplan.Application.Features.Projects.Commands;

public sealed record UpdateProjectCommand(Guid Id, string Code, string Name) : IRequest<Result>;
