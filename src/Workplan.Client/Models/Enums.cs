namespace Workplan.Client.Models;

public enum WorkStatus
{
    Draft,
    Assigned,
    InProgress,
    Submitted,
    ApprovedByHoM,
    ApprovedBySiteChief,
    ApprovedByPM,
    Reported
}

public enum WorkerType
{
    RebarFixer,
    GeneralLabor,
    Operators,
    Survey,
    Slinger,
    Formworker,
    ArchitecturalWorker,
    Assembler,
    Welder
}

public enum Unit
{
    None,
    Ton,
    M3,
    M2
}
