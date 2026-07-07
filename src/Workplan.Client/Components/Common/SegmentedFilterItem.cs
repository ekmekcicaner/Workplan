namespace Workplan.Client.Components;

public sealed record SegmentedFilterItem<TValue>(TValue Value, string Label, int? Count = null);
