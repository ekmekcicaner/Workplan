using Mediator;
using Workplan.SharedKernel.Common;

namespace Workplan.Application.Features.CrewRegions.Commands;

public sealed record SetCrewRegionActiveCommand(Guid Id, bool IsActive) : IRequest<Result>;
