using Mediator;
using Workplan.SharedKernel.Common;

namespace Workplan.Application.Features.DailyPlans.Commands;

// T-1: Teknik Ofis'in aylık üretim planından yararlanarak ustabaşı ve ekiple oluşturduğu günlük plan.
// Birim, seçilen en alt seviye (SSToW) iş kalemi tipinden türetilir; burada ayrıca seçilmez.
public sealed record CreateDailyPlanCommand(
    Guid ProjectId,
    Guid CrewRegionId,
    Guid LocationId,
    Guid WorkItemTypeId,
    DateOnly WorkDate,
    decimal PlannedQuantity,
    decimal PlannedManDay,
    Guid AssignedHoMId) : IRequest<Result<Guid>>;
