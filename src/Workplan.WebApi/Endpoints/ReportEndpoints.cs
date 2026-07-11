using Mediator;
using Workplan.Application.Features.Reports.Queries.GetDailyReport;
using Workplan.WebApi.Common;
using Roles = Workplan.SharedKernel.Auth.Roles;

namespace Workplan.WebApi.Endpoints;

public static class ReportEndpoints
{
    public static IEndpointRouteBuilder MapReportEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/reports").WithTags("Reports");

        group.MapGet("/daily",
                async (
                    DateOnly? workDate,
                    Guid? projectId,
                    Guid? crewRegionId,
                    Guid? headOfMasterId,
                    ISender sender,
                    CancellationToken ct)
                    => (await sender.Send(
                        new GetDailyReportQuery(workDate, projectId, crewRegionId, headOfMasterId), ct)).ToApiResult())
            .RequireAuthorization(p => p.RequireRole(
                Roles.SystemAdmin,
                Roles.TechnicalOfficeEngineer,
                Roles.SiteChief,
                Roles.ProjectManager));

        return app;
    }
}
