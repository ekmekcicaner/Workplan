using Mediator;
using Workplan.SharedKernel.Common;

namespace Workplan.Application.Features.Projects.Commands;

public sealed record CreateProjectCommand(string Code, string Name, Guid? PmUserId) : IRequest<Result<Guid>>;
