using Workplan.SharedKernel.Common;

namespace Workplan.Domain.ValueObjects;

public record Quantity
{
    public decimal Value { get; }
    public Unit Unit { get; }

    private Quantity(decimal value, Unit unit)
    {
        Value = value;
        Unit = unit;
    }

    public static Result<Quantity> Create(decimal value, Unit unit)
    {
        if (value < 0)
            return Error.Validation("Miktar değeri negatif olamaz.");

        return new Quantity(value, unit);
    }
}