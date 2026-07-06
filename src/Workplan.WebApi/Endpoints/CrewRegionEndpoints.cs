using Mediator;
using Workplan.Application.Features.CrewRegions.Commands;
using Workplan.Application.Features.CrewRegions.Queries.GetCrewRegionById;
using Workplan.Application.Features.CrewRegions.Queries.GetCrewRegionsByProject;
using Workplan.WebApi.Common;
using Roles = Workplan.SharedKernel.Auth.Roles;

namespace Workplan.WebApi.Endpoints;

public static class CrewRegionEndpoints
{
    public static IEndpointRouteBuilder MapCrewRegionEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/crew-regions").WithTags("CrewRegions");

        group.MapPost("/", async (CreateCrewRegionCommand command, ISender sender, CancellationToken ct)
                => (await sender.Send(command, ct)).ToApiResult())
            .RequireAuthorization(p => p.RequireRole(Roles.SystemAdmin, Roles.TechnicalOfficeEngineer));

        group.MapGet("/by-project/{projectId:guid}",
                async (Guid projectId, bool? includeInactive, ISender sender, CancellationToken ct)
                    => (await sender.Send(new GetCrewRegionsByProjectQuery(projectId, includeInactive ?? false), ct))
                        .ToApiResult())
            .RequireAuthorization();

        group.MapGet("/{id:guid}", async (Guid id, ISender sender, CancellationToken ct)
                => (await sender.Send(new GetCrewRegionByIdQuery(id), ct)).ToApiResult())
            .RequireAuthorization();

        group.MapPut("/{id:guid}", async (Guid id, UpdateCrewRegionRequest request, ISender sender, CancellationToken ct)
                => (await sender.Send(new UpdateCrewRegionCommand(id, request.Code, request.Name), ct)).ToApiResult())
            .RequireAuthorization(p => p.RequireRole(Roles.SystemAdmin, Roles.TechnicalOfficeEngineer));

        group.MapPost("/{id:guid}/activation",
                async (Guid id, SetActiveRequest request, ISender sender, CancellationToken ct)
                    => (await sender.Send(new SetCrewRegionActiveCommand(id, request.IsActive), ct)).ToApiResult())
            .RequireAuthorization(p => p.RequireRole(Roles.SystemAdmin, Roles.TechnicalOfficeEngineer));

        group.MapPost("/{id:guid}/assign-site-chief",
                async (Guid id, AssignSiteChiefRequest request, ISender sender, CancellationToken ct)
                    => (await sender.Send(new AssignSiteChiefCommand(id, request.SiteChiefUserId), ct)).ToApiResult())
            .RequireAuthorization(p => p.RequireRole(Roles.SystemAdmin, Roles.TechnicalOfficeEngineer));

        group.MapPost("/{id:guid}/assign-tech-office",
                async (Guid id, AssignTechOfficeRequest request, ISender sender, CancellationToken ct)
                    => (await sender.Send(new AssignTechOfficeCommand(id, request.TechOfficeUserId), ct)).ToApiResult())
            .RequireAuthorization(p => p.RequireRole(Roles.SystemAdmin, Roles.TechnicalOfficeEngineer));

        return app;
    }
}

public sealed record UpdateCrewRegionRequest(string Code, string Name);

public sealed record AssignSiteChiefRequest(Guid SiteChiefUserId);

public sealed record AssignTechOfficeRequest(Guid TechOfficeUserId);
