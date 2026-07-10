using System.Reflection;
using FluentAssertions;
using NetArchTest.Rules;
using Xunit;

namespace Workplan.Architecture.Tests;

public class LayerDependencyTests
{
    private static readonly Assembly SharedKernelAssembly = typeof(Workplan.SharedKernel.Common.Result).Assembly;
    private static readonly Assembly DomainAssembly = typeof(Workplan.Domain.Entities.DailyPlan).Assembly;
    private static readonly Assembly ApplicationAssembly = typeof(Workplan.Application.DependencyInjection).Assembly;
    private static readonly Assembly InfrastructureAssembly = typeof(Workplan.Infrastructure.DependencyInjection).Assembly;

    [Fact]
    [Trait("Category", "Architecture")]
    public void Domain_does_not_depend_on_any_framework_or_outer_layer()
    {
        var result = Types.InAssembly(DomainAssembly)
            .Should()
            .NotHaveDependencyOnAny(
                "Mediator",
                "FluentValidation",
                "Microsoft.EntityFrameworkCore",
                "Microsoft.AspNetCore",
                "Workplan.Application",
                "Workplan.Infrastructure",
                "Workplan.WebApi")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(BuildFailureMessage(result));
    }

    [Fact]
    [Trait("Category", "Architecture")]
    public void SharedKernel_does_not_depend_on_any_other_project_or_framework()
    {
        var result = Types.InAssembly(SharedKernelAssembly)
            .Should()
            .NotHaveDependencyOnAny(
                "Mediator",
                "FluentValidation",
                "Microsoft.EntityFrameworkCore",
                "Microsoft.AspNetCore",
                "Workplan.Domain",
                "Workplan.Application",
                "Workplan.Infrastructure",
                "Workplan.WebApi")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(BuildFailureMessage(result));
    }

    [Fact]
    [Trait("Category", "Architecture")]
    public void Application_does_not_depend_on_infrastructure_or_webapi()
    {
        var result = Types.InAssembly(ApplicationAssembly)
            .Should()
            .NotHaveDependencyOnAny("Workplan.Infrastructure", "Workplan.WebApi")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(BuildFailureMessage(result));
    }

    [Fact]
    [Trait("Category", "Architecture")]
    public void Infrastructure_does_not_depend_on_webapi()
    {
        var result = Types.InAssembly(InfrastructureAssembly)
            .Should()
            .NotHaveDependencyOn("Workplan.WebApi")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(BuildFailureMessage(result));
    }

    private static string BuildFailureMessage(TestResult result) =>
        result.FailingTypes is null
            ? "Beklenmeyen katman bağımlılığı bulundu."
            : "Beklenmeyen katman bağımlılığı: " + string.Join(", ", result.FailingTypes.Select(t => t.FullName));
}
