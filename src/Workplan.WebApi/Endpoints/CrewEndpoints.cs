using Mediator;
using Workplan.Application.Features.Crews.Commands;
using Workplan.Application.Features.Crews.Queries.GetCrewsByLocation;
using Workplan.Domain.Enums;
using Workplan.WebApi.Common;
using Roles = Workplan.SharedKernel.Auth.Roles;

namespace Workplan.WebApi.Endpoints;

public static class CrewEndpoints
{
    public static IEndpointRouteBuilder MapCrewEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/crews").WithTags("Crews");

        group.MapPost("/", async (CreateCrewCommand command, ISender sender, CancellationToken ct)
                => (await sender.Send(command, ct)).ToApiResult())
            .RequireAuthorization(p => p.RequireRole(Roles.SystemAdmin, Roles.HeadOfMaster));

        group.MapPost("/{id:guid}/members",
                async (Guid id, AddCrewMemberRequest request, ISender sender, CancellationToken ct)
                    => (await sender.Send(new AddCrewMemberCommand(id, request.WorkerType, request.PersonnelRef), ct))
                        .ToApiResult())
            .RequireAuthorization(p => p.RequireRole(Roles.SystemAdmin, Roles.HeadOfMaster));

        group.MapGet("/by-location/{locationId:guid}", async (Guid locationId, ISender sender, CancellationToken ct)
                => (await sender.Send(new GetCrewsByLocationQuery(locationId), ct)).ToApiResult())
            .RequireAuthorization();

        return app;
    }
}

public sealed record AddCrewMemberRequest(WorkerType WorkerType, string PersonnelRef);
