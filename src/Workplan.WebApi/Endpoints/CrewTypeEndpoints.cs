using Mediator;
using Workplan.Application.Features.CrewTypes.Commands;
using Workplan.Application.Features.CrewTypes.Queries.GetCrewTypes;
using Workplan.WebApi.Common;
using Roles = Workplan.SharedKernel.Auth.Roles;

namespace Workplan.WebApi.Endpoints;

public static class CrewTypeEndpoints
{
    public static IEndpointRouteBuilder MapCrewTypeEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/crew-types").WithTags("CrewTypes");

        group.MapGet("/", async (bool? includeInactive, ISender sender, CancellationToken ct)
                => (await sender.Send(new GetCrewTypesQuery(includeInactive ?? false), ct)).ToApiResult())
            .RequireAuthorization();

        group.MapPost("/", async (CreateCrewTypeCommand command, ISender sender, CancellationToken ct)
                => (await sender.Send(command, ct)).ToApiResult())
            .RequireAuthorization(p => p.RequireRole(Roles.SystemAdmin, Roles.TechnicalOfficeEngineer));

        group.MapPut("/{id:guid}", async (Guid id, UpdateCrewTypeRequest request, ISender sender, CancellationToken ct)
                => (await sender.Send(new UpdateCrewTypeCommand(id, request.Name), ct)).ToApiResult())
            .RequireAuthorization(p => p.RequireRole(Roles.SystemAdmin, Roles.TechnicalOfficeEngineer));

        group.MapPost("/{id:guid}/activation",
                async (Guid id, SetActiveRequest request, ISender sender, CancellationToken ct)
                    => (await sender.Send(new SetCrewTypeActiveCommand(id, request.IsActive), ct)).ToApiResult())
            .RequireAuthorization(p => p.RequireRole(Roles.SystemAdmin, Roles.TechnicalOfficeEngineer));

        return app;
    }
}

public sealed record UpdateCrewTypeRequest(string Name);
