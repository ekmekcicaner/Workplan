using Mediator;
using Workplan.SharedKernel.Common;

namespace Workplan.Application.Features.CrewRegions.Commands;

public sealed record AssignSiteChiefCommand(Guid CrewRegionId, Guid SiteChiefUserId) : IRequest<Result>;
