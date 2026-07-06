using Mediator;
using Workplan.Application.Features.WorkItemTypes.Commands;
using Workplan.Application.Features.WorkItemTypes.Queries.GetWorkItemTypeTree;
using Workplan.WebApi.Common;
using Roles = Workplan.SharedKernel.Auth.Roles;
using WorkUnit = Workplan.Domain.ValueObjects.Unit;

namespace Workplan.WebApi.Endpoints;

public static class WorkItemTypeEndpoints
{
    public static IEndpointRouteBuilder MapWorkItemTypeEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/work-item-types").WithTags("WorkItemTypes");

        group.MapPost("/", async (CreateWorkItemTypeCommand command, ISender sender, CancellationToken ct)
                => (await sender.Send(command, ct)).ToApiResult())
            .RequireAuthorization(p => p.RequireRole(Roles.SystemAdmin, Roles.TechnicalOfficeEngineer));

        group.MapGet("/tree", async (bool? includeInactive, ISender sender, CancellationToken ct)
                => (await sender.Send(new GetWorkItemTypeTreeQuery(includeInactive ?? false), ct)).ToApiResult())
            .RequireAuthorization();

        group.MapPut("/{id:guid}", async (Guid id, UpdateWorkItemTypeRequest request, ISender sender, CancellationToken ct)
                => (await sender.Send(new UpdateWorkItemTypeCommand(id, request.Name, request.Unit), ct)).ToApiResult())
            .RequireAuthorization(p => p.RequireRole(Roles.SystemAdmin, Roles.TechnicalOfficeEngineer));

        group.MapPost("/{id:guid}/activation",
                async (Guid id, SetActiveRequest request, ISender sender, CancellationToken ct)
                    => (await sender.Send(new SetWorkItemTypeActiveCommand(id, request.IsActive), ct)).ToApiResult())
            .RequireAuthorization(p => p.RequireRole(Roles.SystemAdmin, Roles.TechnicalOfficeEngineer));

        return app;
    }
}

public sealed record UpdateWorkItemTypeRequest(string Name, WorkUnit Unit = WorkUnit.None);
