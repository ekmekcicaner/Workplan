namespace Workplan.SharedKernel.Common;

// HTTP status kodu eşlemesi bu enum üzerinden yapılır (bkz. ResultExtensions.ToProblem);
// Error.Code stringi yalnızca API sözleşmesi (wire format) için korunur.
public enum ErrorType
{
    Validation,
    NotFound,
    Unauthorized,
    ScopeMismatch,
    OutOfOrder,
    TerminalImmutable,
    Sentinel,
    EmptySubmit
}
