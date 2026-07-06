using Mediator;
using Workplan.Application.Features.Auth.Commands;
using Workplan.Application.Interfaces;
using Workplan.WebApi.Common;
using Roles = Workplan.SharedKernel.Auth.Roles;

namespace Workplan.WebApi.Endpoints;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth").WithTags("Auth");

        group.MapPost("/login", async (LoginCommand command, ISender sender, CancellationToken ct)
            => (await sender.Send(command, ct)).ToApiResult());

        group.MapPost("/refresh", async (RefreshTokenCommand command, ISender sender, CancellationToken ct)
            => (await sender.Send(command, ct)).ToApiResult());

        group.MapPost("/revoke", async (RevokeTokenCommand command, ISender sender, CancellationToken ct)
                => (await sender.Send(command, ct)).ToApiResult())
            .RequireAuthorization();

        group.MapPost("/register", async (RegisterCommand command, ISender sender, CancellationToken ct)
                => (await sender.Send(command, ct)).ToApiResult())
            .RequireAuthorization(p => p.RequireRole(Roles.SystemAdmin));

        group.MapGet("/me", (ICurrentUserService currentUser)
                => Results.Ok(new { currentUser.UserId, currentUser.Roles }))
            .RequireAuthorization();

        return app;
    }
}
