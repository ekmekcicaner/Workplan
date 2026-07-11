using FluentAssertions;
using Workplan.Domain.Common;
using Xunit;

namespace Workplan.Domain.Tests;

public sealed class EntityIdTests
{
    [Fact]
    [Trait("Category", "Unit")]
    public void New_ReturnsVersion7Guid()
    {
        var id = EntityId.New();

        id.Version.Should().Be(7);
        id.Should().NotBe(Guid.Empty);
    }
}
