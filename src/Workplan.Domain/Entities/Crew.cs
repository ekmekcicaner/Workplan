using Workplan.Domain.Common;
using Workplan.SharedKernel.Common;

namespace Workplan.Domain.Entities;

public class Crew : Entity<Guid>
{
    public Guid LocationId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public Guid CreatedByHoMId { get; private set; }

    public Location? Location { get; private set; }

    private readonly List<CrewMember> _members = new();
    public IReadOnlyCollection<CrewMember> Members => _members.AsReadOnly();

    private Crew()
    {
    }

    private Crew(Guid id, Guid locationId, string name, Guid createdByHoMId)
    {
        Id = id;
        LocationId = locationId;
        Name = name;
        CreatedByHoMId = createdByHoMId;
    }

    public static Result<Crew> Create(Guid locationId, string name, Guid createdByHoMId)
    {
        if (locationId == Guid.Empty) return Result<Crew>.Fail(Error.Validation("Lokasyon ID boş olamaz."));
        if (string.IsNullOrWhiteSpace(name)) return Result<Crew>.Fail(Error.Validation("Ekip adı boş olamaz."));
        if (createdByHoMId == Guid.Empty)
            return Result<Crew>.Fail(Error.Validation("Ekibi oluşturan Head of Master ID boş olamaz."));

        return new Crew(Guid.NewGuid(), locationId, name.Trim(), createdByHoMId);
    }

    public Result<CrewMember> AddMember(Enums.WorkerType workerType, string personnelRef)
    {
        if (string.IsNullOrWhiteSpace(personnelRef))
            return Result<CrewMember>.Fail(Error.Validation("Personel bilgisi boş olamaz."));

        var member = CrewMember.Create(Id, workerType, personnelRef.Trim());
        if (member.IsFailure) return member;

        _members.Add(member.Value);
        return member;
    }
}
