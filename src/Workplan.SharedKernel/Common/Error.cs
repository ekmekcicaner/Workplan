namespace Workplan.SharedKernel.Common;

public sealed record Error(string Code, string Message, ErrorType Type, IEnumerable<string>? Details = null)
{
    public static readonly Error None = new(string.Empty, string.Empty, ErrorType.Validation);

    public static Error NotFound(string message) => new("not_found", message, ErrorType.NotFound);
    public static Error OutOfOrder(string message) => new("out_of_order", message, ErrorType.OutOfOrder);
    public static Error ScopeMismatch(string message) => new("scope_mismatch", message, ErrorType.ScopeMismatch);
    public static Error Unauthorized(string message) => new("unauthorized", message, ErrorType.Unauthorized);
    public static Error Sentinel(string message) => new("sentinel", message, ErrorType.Sentinel);
    public static Error EmptySubmit(string message) => new("empty_submit", message, ErrorType.EmptySubmit);
    public static Error TerminalImmutable(string message) => new("terminal_immutable", message, ErrorType.TerminalImmutable);

    public static Error Validation(string message, IEnumerable<string>? details = null)
        => new("Validation", message, ErrorType.Validation, details);
}
