using Mediator;
using Workplan.SharedKernel.Common;

namespace Workplan.Application.Features.Reports.Queries.GetDailyReport;

public sealed record GetDailyReportQuery(
    DateOnly? WorkDate,
    Guid? ProjectId,
    Guid? CrewRegionId,
    Guid? HeadOfMasterId) : IRequest<Result<DailyReportDto>>;
