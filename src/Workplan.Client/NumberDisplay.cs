using System.Globalization;

namespace Workplan.Client;

public static class NumberDisplay
{
    public static string Format(decimal value) => value.ToString("0.####", CultureInfo.CurrentCulture);

    public static string Format(decimal? value) => value is { } number ? Format(number) : "-";
}
