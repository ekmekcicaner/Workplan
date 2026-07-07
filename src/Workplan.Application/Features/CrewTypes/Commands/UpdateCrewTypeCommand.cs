using Mediator;
using Workplan.SharedKernel.Common;

namespace Workplan.Application.Features.CrewTypes.Commands;

public sealed record UpdateCrewTypeCommand(Guid Id, string Name) : IRequest<Result>;
