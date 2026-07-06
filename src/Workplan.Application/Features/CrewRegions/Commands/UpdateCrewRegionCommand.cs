using Mediator;
using Workplan.SharedKernel.Common;

namespace Workplan.Application.Features.CrewRegions.Commands;

public sealed record UpdateCrewRegionCommand(Guid Id, string Code, string Name) : IRequest<Result>;
