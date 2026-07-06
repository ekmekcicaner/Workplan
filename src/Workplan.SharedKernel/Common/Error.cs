namespace Workplan.SharedKernel.Common;

public sealed record Error(string Code, string Message, IEnumerable<string>? Details = null)
{
    public static readonly Error None = new(string.Empty, string.Empty);
    public static Error NotFound(string message) => new("not_found", message); // 409 — sıra ihlali

    public static Error OutOfOrder(string message) => new("out_of_order", message); // 409 — sıra ihlali
    public static Error ScopeMismatch(string message) => new("scope_mismatch", message); // 403 — scope ihlali
    public static Error Unauthorized(string message) => new("unauthorized", message); // 401 — kimlik doğrulama yok
    public static Error Sentinel(string message) => new("sentinel", message); // 422 — negatif/sentinel
    public static Error EmptySubmit(string message) => new("empty_submit", message); // 422 — boş submit
    public static Error TerminalImmutable(string message) => new("terminal_immutable", message);

    public static Error Validation(string message, IEnumerable<string>? details = null)
        => new("Validation", message, details);
}