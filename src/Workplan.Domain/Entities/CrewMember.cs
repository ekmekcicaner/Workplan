using Workplan.Domain.Common;
using Workplan.Domain.Enums;
using Workplan.SharedKernel.Common;

namespace Workplan.Domain.Entities;

public class CrewMember : Entity<Guid>
{
    public Guid CrewId { get; private set; }
    public WorkerType WorkerType { get; private set; }

    // Gerçek personel/İK master verisi henüz modellenmedi; ileride bir Employee/Identity
    // tablosuna FK olacak yer tutucu (sicil no, isim vb. serbest metin).
    public string PersonnelRef { get; private set; } = string.Empty;

    public Crew? Crew { get; private set; }

    private CrewMember()
    {
    }

    private CrewMember(Guid id, Guid crewId, WorkerType workerType, string personnelRef)
    {
        Id = id;
        CrewId = crewId;
        WorkerType = workerType;
        PersonnelRef = personnelRef;
    }

    public static Result<CrewMember> Create(Guid crewId, WorkerType workerType, string personnelRef)
    {
        if (crewId == Guid.Empty) return Result<CrewMember>.Fail(Error.Validation("Ekip ID boş olamaz."));
        if (string.IsNullOrWhiteSpace(personnelRef))
            return Result<CrewMember>.Fail(Error.Validation("Personel bilgisi boş olamaz."));

        return new CrewMember(Guid.NewGuid(), crewId, workerType, personnelRef);
    }
}
