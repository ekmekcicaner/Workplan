using Mediator;
using Workplan.Application.Features.Locations.Commands;
using Workplan.Application.Features.Locations.Queries.GetLocationById;
using Workplan.Application.Features.Locations.Queries.GetLocationsByRegion;
using Workplan.WebApi.Common;
using Roles = Workplan.SharedKernel.Auth.Roles;

namespace Workplan.WebApi.Endpoints;

public static class LocationEndpoints
{
    public static IEndpointRouteBuilder MapLocationEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/locations").WithTags("Locations");

        group.MapPost("/", async (CreateLocationCommand command, ISender sender, CancellationToken ct)
                => (await sender.Send(command, ct)).ToApiResult())
            .RequireAuthorization(p => p.RequireRole(Roles.SystemAdmin, Roles.TechnicalOfficeEngineer));

        group.MapPost("/{id:guid}/assign-head-of-master",
                async (Guid id, AssignHeadOfMasterRequest request, ISender sender, CancellationToken ct)
                    => (await sender.Send(new AssignHeadOfMasterCommand(id, request.HeadOfMasterUserId), ct)).ToApiResult())
            .RequireAuthorization(p => p.RequireRole(
                Roles.SystemAdmin, Roles.TechnicalOfficeEngineer, Roles.SiteChief));

        group.MapGet("/by-region/{crewRegionId:guid}",
                async (Guid crewRegionId, bool? includeInactive, ISender sender, CancellationToken ct)
                    => (await sender.Send(new GetLocationsByRegionQuery(crewRegionId, includeInactive ?? false), ct))
                        .ToApiResult())
            .RequireAuthorization();

        group.MapGet("/{id:guid}", async (Guid id, ISender sender, CancellationToken ct)
                => (await sender.Send(new GetLocationByIdQuery(id), ct)).ToApiResult())
            .RequireAuthorization();

        group.MapPut("/{id:guid}", async (Guid id, UpdateLocationRequest request, ISender sender, CancellationToken ct)
                => (await sender.Send(new UpdateLocationCommand(id, request.Name), ct)).ToApiResult())
            .RequireAuthorization(p => p.RequireRole(Roles.SystemAdmin, Roles.TechnicalOfficeEngineer));

        group.MapPost("/{id:guid}/activation",
                async (Guid id, SetActiveRequest request, ISender sender, CancellationToken ct)
                    => (await sender.Send(new SetLocationActiveCommand(id, request.IsActive), ct)).ToApiResult())
            .RequireAuthorization(p => p.RequireRole(Roles.SystemAdmin, Roles.TechnicalOfficeEngineer));

        return app;
    }
}

public sealed record AssignHeadOfMasterRequest(Guid HeadOfMasterUserId);

public sealed record UpdateLocationRequest(string Name);
