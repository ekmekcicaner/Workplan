using FluentValidation;
using Mediator;
using Microsoft.Extensions.DependencyInjection;
using Workplan.Application.Common;
using Workplan.Application.Interfaces;

namespace Workplan.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediator(options => options.ServiceLifetime = ServiceLifetime.Scoped);
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        services.AddScoped<IAccessScopeService, AccessScopeService>();

        return services;
    }
}
