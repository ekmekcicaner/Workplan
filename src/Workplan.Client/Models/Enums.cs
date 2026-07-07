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

public enum Unit
{
    None,
    Ton,
    M3,
    M2
}

public enum DailyPlanCommentKind
{
    Progress,
    Rejection
}
