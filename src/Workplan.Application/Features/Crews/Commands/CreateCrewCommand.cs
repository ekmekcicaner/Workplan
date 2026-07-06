using Mediator;
using Workplan.SharedKernel.Common;

namespace Workplan.Application.Features.Crews.Commands;

public sealed record CreateCrewCommand(Guid LocationId, string Name) : IRequest<Result<Guid>>;
