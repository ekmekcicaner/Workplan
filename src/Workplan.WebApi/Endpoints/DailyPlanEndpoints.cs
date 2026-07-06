using Mediator;
using Workplan.Application.Features.DailyPlans.Commands;
using Workplan.Application.Features.DailyPlans.Queries.GetApprovedDailyPlans;
using Workplan.Application.Features.DailyPlans.Queries.GetDailyPlanByHeadMaster;
using Workplan.Application.Features.DailyPlans.Queries.GetDailyPlanById;
using Workplan.Application.Features.DailyPlans.Queries.GetPlansAwaitingMyApproval;
using Workplan.WebApi.Common;
using Roles = Workplan.SharedKernel.Auth.Roles;

namespace Workplan.WebApi.Endpoints;

public static class DailyPlanEndpoints
{
    public static IEndpointRouteBuilder MapDailyPlanEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/daily-plans").WithTags("DailyPlans");

        // T-1: Teknik Ofis günlük planı ustabaşı ve ekiple oluşturur.
        group.MapPost("/", async (CreateDailyPlanCommand command, ISender sender, CancellationToken ct)
                => (await sender.Send(command, ct)).ToApiResult())
            .RequireAuthorization(p => p.RequireRole(Roles.SystemAdmin, Roles.TechnicalOfficeEngineer));

        // T0: Head of Master kendisine atanan işi, seçtiği ekiple başlatır.
        group.MapPost("/{id:guid}/start",
                async (Guid id, StartWorkRequest request, ISender sender, CancellationToken ct)
                    => (await sender.Send(new StartWorkCommand(id, request.CrewId), ct)).ToApiResult())
            .RequireAuthorization(p => p.RequireRole(Roles.HeadOfMaster));

        // Gün Sonu: gerçekleşen değerler girilip onaya sunulur.
        group.MapPost("/{id:guid}/submit-progress",
                async (Guid id, SubmitProgressRequest request, ISender sender, CancellationToken ct)
                    => (await sender.Send(
                        new SubmitProgressCommand(
                            id, request.FactQuantity, request.FactManDay, request.Overtime,
                            request.Comment),
                        ct)).ToApiResult())
            .RequireAuthorization(p => p.RequireRole(Roles.HeadOfMaster));

        // Onay: Site Chief -> PM. Onaylayan rol/kullanıcı JWT claim'lerinden çözülür.
        group.MapPost("/{id:guid}/approve",
                async (Guid id, ISender sender, CancellationToken ct)
                    => (await sender.Send(new ApproveCommand(id), ct)).ToApiResult())
            .RequireAuthorization(p => p.RequireRole(Roles.SiteChief, Roles.ProjectManager));

        group.MapPost("/{id:guid}/reject",
                async (Guid id, RejectRequest request, ISender sender, CancellationToken ct)
                    => (await sender.Send(new RejectCommand(id, request.Reason), ct)).ToApiResult())
            .RequireAuthorization(p => p.RequireRole(Roles.SiteChief, Roles.ProjectManager));

        group.MapGet("/by-head-of-master/{headOfMasterUserId:guid}",
                async (Guid headOfMasterUserId, ISender sender, CancellationToken ct)
                    => (await sender.Send(new GetDailyPlanByHeadMasterQuery(headOfMasterUserId), ct)).ToApiResult())
            .RequireAuthorization();

        // Ana sayfa: giriş yapan kullanıcının onay rolüne göre onay bekleyen işler.
        group.MapGet("/awaiting-approval", async (ISender sender, CancellationToken ct)
                => (await sender.Send(new GetPlansAwaitingMyApprovalQuery(), ct)).ToApiResult())
            .RequireAuthorization();

        group.MapGet("/{id:guid}", async (Guid id, ISender sender, CancellationToken ct)
                => (await sender.Send(new GetDailyPlanByIdQuery(id), ct)).ToApiResult())
            .RequireAuthorization();

        // Raporlama seam'i: Power BI'ın DirectQuery ile çekebileceği onaylanmış kayıtlar.
        group.MapGet("/approved", async (ISender sender, CancellationToken ct)
                => (await sender.Send(new GetApprovedDailyPlansQuery(), ct)).ToApiResult())
            .RequireAuthorization();

        return app;
    }
}

public sealed record StartWorkRequest(Guid CrewId);

public sealed record SubmitProgressRequest(
    decimal? FactQuantity,
    decimal? FactManDay,
    decimal? Overtime,
    string? Comment);

public sealed record RejectRequest(string Reason);
