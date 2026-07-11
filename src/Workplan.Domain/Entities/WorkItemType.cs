using Workplan.Domain.Common;
using Workplan.Domain.ValueObjects;
using Workplan.SharedKernel.Common;

namespace Workplan.Domain.Entities;

public class WorkItemType : Entity<Guid>
{
    public string Name { get; private set; } = string.Empty;
    public Guid? ParentId { get; private set; }
    public int Level { get; private set; } // 0 = ToW, 1 = SToW, 2 = SSToW
    public bool IsActive { get; private set; } = true;

    // Sadece en alt seviye (SSToW, Level == MaxLevel) düğümlerde anlamlıdır; üst seviyelerde None kalır.
    public Unit Unit { get; private set; } = Unit.None;

    public WorkItemType? Parent { get; private set; }
    private readonly List<WorkItemType> _children = new();
    public IReadOnlyCollection<WorkItemType> Children => _children.AsReadOnly();

    private const int MaxLevel = 2; // ToW -> SToW -> SSToW

    private WorkItemType() { }

    private WorkItemType(Guid id, string name, Guid? parentId, int level, Unit unit)
    {
        Id = id;
        Name = name;
        ParentId = parentId;
        Level = level;
        Unit = unit;
        IsActive = true;
    }

    // Fabrika Metodu. parentLevel çağıran taraf (parent zaten yüklenmişse) tarafından verilir.
    public static Result<WorkItemType> Create(string name, Guid? parentId = null, int? parentLevel = null, Unit unit = Unit.None)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result<WorkItemType>.Fail(Error.Validation("İş tipi adı boş olamaz."));

        if (parentId is null)
        {
            var rootUnitCheck = ValidateUnitForLevel(0, unit);
            if (rootUnitCheck.IsFailure) return Result<WorkItemType>.Fail(rootUnitCheck.Error);
            return new WorkItemType(EntityId.New(), name.Trim(), null, 0, unit);
        }

        if (parentLevel is null)
            return Result<WorkItemType>.Fail(Error.Validation("Üst iş tipinin seviyesi bilinmeden alt iş tipi oluşturulamaz."));

        if (parentLevel.Value >= MaxLevel)
            return Result<WorkItemType>.Fail(Error.Validation("ToW -> SToW -> SSToW hiyerarşisi en fazla 3 seviye olabilir."));

        var level = parentLevel.Value + 1;
        var unitCheck = ValidateUnitForLevel(level, unit);
        if (unitCheck.IsFailure) return Result<WorkItemType>.Fail(unitCheck.Error);

        return new WorkItemType(EntityId.New(), name.Trim(), parentId, level, unit);
    }

    public Result Rename(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result.Fail(Error.Validation("İş tipi adı boş olamaz."));
        Name = name.Trim();
        return Result.Ok();
    }

    public Result SetUnit(Unit unit)
    {
        var check = ValidateUnitForLevel(Level, unit);
        if (check.IsFailure) return check;

        Unit = unit;
        return Result.Ok();
    }

    private static Result ValidateUnitForLevel(int level, Unit unit)
    {
        if (level == MaxLevel)
        {
            return unit == Unit.None
                ? Result.Fail(Error.Validation("En alt seviye (SSToW) iş tipi için birim seçilmelidir."))
                : Result.Ok();
        }

        return unit != Unit.None
            ? Result.Fail(Error.Validation("Birim yalnızca en alt seviye (SSToW) iş tiplerine atanabilir."))
            : Result.Ok();
    }

    public void Activate() => IsActive = true;

    public void Deactivate() => IsActive = false;
}
