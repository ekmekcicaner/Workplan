using Mediator;
using Workplan.Application.Features.DailyPlans.Commands;
using Workplan.Application.Features.DailyPlans.Queries.GetApprovedDailyPlans;
using Workplan.Application.Features.DailyPlans.Queries.GetApprovalQueue;
using Workplan.Application.Features.DailyPlans.Queries.GetDailyPlanDetail;
using Workplan.Application.Features.DailyPlans.Queries.GetDailyPlanByHeadMaster;
using Workplan.Application.Features.DailyPlans.Queries.GetDailyPlanById;
using Workplan.Application.Features.DailyPlans.Queries.GetDailyTrackingOptions;
using Workplan.Application.Features.DailyPlans.Queries.GetDailyTrackingPlans;
using Workplan.Application.Features.DailyPlans.Queries.GetMyWorkDailyPlans;
using Workplan.Application.Features.DailyPlans.Queries.GetPlansAwaitingMyApproval;
using Workplan.Domain.Enums;
using Workplan.SharedKernel.Common;
using Workplan.WebApi.Common;
using Roles = Workplan.SharedKernel.Auth.Roles;

namespace Workplan.WebApi.Endpoints;

public static class DailyPlanEndpoints
{
    public static IEndpointRouteBuilder MapDailyPlanEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/daily-plans").WithTags("DailyPlans");

        // T-1: Teknik Ofis günlük planı ustabaşı ve ekiple oluşturur.
        group.MapPost("/", async (CreateDailyPlanCommand command, ISender sender, CancellationToken ct)
                => (await sender.Send(command, ct)).ToApiResult())
            .RequireAuthorization(p => p.RequireRole(Roles.SystemAdmin, Roles.TechnicalOfficeEngineer));

        // T0: Head of Master kendisine atanan işi, seçtiği crew type ile başlatır.
        group.MapPost("/{id:guid}/start",
                async (Guid id, StartWorkRequest request, ISender sender, CancellationToken ct)
                    => request.CrewTypeId is { } crewTypeId
                        ? (await sender.Send(new StartWorkCommand(id, crewTypeId), ct)).ToApiResult()
                        : Result.Fail(Error.Validation("Ekip tipi seçilmelidir.")).ToApiResult())
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

        group.MapGet("/my-work", async (ISender sender, CancellationToken ct)
                => (await sender.Send(new GetMyWorkDailyPlansQuery(), ct)).ToApiResult())
            .RequireAuthorization(p => p.RequireRole(Roles.HeadOfMaster));

        group.MapGet("/approval-queue", async (ISender sender, CancellationToken ct)
                => (await sender.Send(new GetApprovalQueueQuery(), ct)).ToApiResult())
            .RequireAuthorization(p => p.RequireRole(Roles.SiteChief, Roles.ProjectManager));

        group.MapGet("/tracking/options", async (ISender sender, CancellationToken ct)
                => (await sender.Send(new GetDailyTrackingOptionsQuery(), ct)).ToApiResult())
            .RequireAuthorization(p => p.RequireRole(
                Roles.SystemAdmin,
                Roles.TechnicalOfficeEngineer,
                Roles.ProjectManager,
                Roles.SiteChief,
                Roles.HeadOfMaster));

        group.MapGet("/tracking",
                async (
                    DateOnly workDate,
                    Guid[]? projectIds,
                    Guid[]? crewRegionIds,
                    Guid[]? locationIds,
                    Guid[]? headOfMasterIds,
                    Guid[]? siteChiefIds,
                    Guid[]? projectManagerIds,
                    WorkStatus[]? statuses,
                    ISender sender,
                    CancellationToken ct)
                    => (await sender.Send(
                        new GetDailyTrackingPlansQuery(
                            workDate,
                            projectIds ?? [],
                            crewRegionIds ?? [],
                            locationIds ?? [],
                            headOfMasterIds ?? [],
                            siteChiefIds ?? [],
                            projectManagerIds ?? [],
                            statuses ?? []),
                        ct)).ToApiResult())
            .RequireAuthorization(p => p.RequireRole(
                Roles.SystemAdmin,
                Roles.TechnicalOfficeEngineer,
                Roles.ProjectManager,
                Roles.SiteChief,
                Roles.HeadOfMaster));

        // Ana sayfa: giriş yapan kullanıcının onay rolüne göre onay bekleyen işler.
        group.MapGet("/awaiting-approval", async (ISender sender, CancellationToken ct)
                => (await sender.Send(new GetPlansAwaitingMyApprovalQuery(), ct)).ToApiResult())
            .RequireAuthorization();

        group.MapGet("/{id:guid}/detail", async (Guid id, ISender sender, CancellationToken ct)
                => (await sender.Send(new GetDailyPlanDetailQuery(id), ct)).ToApiResult())
            .RequireAuthorization();

        group.MapGet("/{id:guid}", async (Guid id, ISender sender, CancellationToken ct)
                => (await sender.Send(new GetDailyPlanByIdQuery(id), ct)).ToApiResult())
            .RequireAuthorization();

        // PM onayı tamamlanmış kayıtlar için uygulama içi okuma modeli.
        group.MapGet("/approved", async (ISender sender, CancellationToken ct)
                => (await sender.Send(new GetApprovedDailyPlansQuery(), ct)).ToApiResult())
            .RequireAuthorization();

        return app;
    }
}

public sealed record StartWorkRequest(Guid? CrewTypeId);

public sealed record SubmitProgressRequest(
    decimal? FactQuantity,
    decimal? FactManDay,
    decimal? Overtime,
    string? Comment);

public sealed record RejectRequest(string Reason);
