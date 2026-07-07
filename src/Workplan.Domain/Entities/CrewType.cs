using Workplan.Domain.Common;
using Workplan.SharedKernel.Common;

namespace Workplan.Domain.Entities;

public class CrewType : Entity<Guid>
{
    public string Name { get; private set; } = string.Empty;
    public bool IsActive { get; private set; } = true;

    private CrewType()
    {
    }

    private CrewType(Guid id, string name)
    {
        Id = id;
        Name = name;
        IsActive = true;
    }

    public static Result<CrewType> Create(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result<CrewType>.Fail(Error.Validation("Ekip tipi adı boş olamaz."));

        return new CrewType(Guid.NewGuid(), name.Trim());
    }

    public Result Rename(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result.Fail(Error.Validation("Ekip tipi adı boş olamaz."));

        Name = name.Trim();
        return Result.Ok();
    }

    public void Activate() => IsActive = true;

    public void Deactivate() => IsActive = false;
}
