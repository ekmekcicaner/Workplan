using Mediator;
using Workplan.SharedKernel.Common;

namespace Workplan.Application.Features.CrewRegions.Commands;

public sealed record AssignTechOfficeCommand(Guid CrewRegionId, Guid TechOfficeUserId) : IRequest<Result>;
