using Workplan.SharedKernel.Common;

namespace Workplan.Domain.ValueObjects;

public record ManDay
{
    public decimal Value { get; }

    private ManDay(decimal value) => Value = value;

    public static Result<ManDay> Create(decimal value)
    {
        if (value < 0) return Error.Validation("Man-Day değeri negatif olamaz.");
        return new ManDay(value);
    }
}