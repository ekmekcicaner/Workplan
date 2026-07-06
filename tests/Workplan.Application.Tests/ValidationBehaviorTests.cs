using FluentAssertions;
using FluentValidation;
using Mediator;
using Workplan.Application.Common;
using Workplan.Application.Features.Projects.Commands;
using Workplan.SharedKernel.Common;
using Xunit;

namespace Workplan.Application.Tests;

public class ValidationBehaviorTests
{
    [Fact]
    [Trait("Category", "Application")]
    public async Task ValidationBehavior_returns_failed_ResultOfT_without_invoking_next_handler()
    {
        var behavior = new ValidationBehavior<CreateProjectCommand, Result<Guid>>(
            [new CreateProjectCommandValidator()]);
        var nextCalled = false;

        var result = await behavior.Handle(
            new CreateProjectCommand("", "", null),
            (_, _) =>
            {
                nextCalled = true;
                return ValueTask.FromResult<Result<Guid>>(Guid.NewGuid());
            },
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Validation");
        nextCalled.Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Application")]
    public async Task ValidationBehavior_invokes_next_when_request_is_valid()
    {
        var expected = Guid.NewGuid();
        var behavior = new ValidationBehavior<CreateProjectCommand, Result<Guid>>(
            [new CreateProjectCommandValidator()]);

        var result = await behavior.Handle(
            new CreateProjectCommand("P01", "Project", null),
            (_, _) => ValueTask.FromResult<Result<Guid>>(expected),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(expected);
    }

    private sealed class AlwaysInvalidCommandValidator : AbstractValidator<AlwaysInvalidCommand>
    {
        public AlwaysInvalidCommandValidator()
        {
            RuleFor(x => x.Name).NotEmpty();
        }
    }

    private sealed record AlwaysInvalidCommand(string Name) : IRequest<Result>;

    [Fact]
    [Trait("Category", "Application")]
    public async Task ValidationBehavior_returns_failed_plain_Result()
    {
        var behavior = new ValidationBehavior<AlwaysInvalidCommand, Result>(
            [new AlwaysInvalidCommandValidator()]);

        var result = await behavior.Handle(
            new AlwaysInvalidCommand(""),
            (_, _) => ValueTask.FromResult(Result.Ok()),
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Validation");
    }
}
