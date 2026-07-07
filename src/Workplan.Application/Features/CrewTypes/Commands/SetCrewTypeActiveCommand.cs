using Mediator;
using Workplan.SharedKernel.Common;

namespace Workplan.Application.Features.CrewTypes.Commands;

public sealed record SetCrewTypeActiveCommand(Guid Id, bool IsActive) : IRequest<Result>;
