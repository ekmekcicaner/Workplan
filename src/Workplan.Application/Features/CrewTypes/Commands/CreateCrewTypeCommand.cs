using Mediator;
using Workplan.SharedKernel.Common;

namespace Workplan.Application.Features.CrewTypes.Commands;

public sealed record CreateCrewTypeCommand(string Name) : IRequest<Result<Guid>>;
