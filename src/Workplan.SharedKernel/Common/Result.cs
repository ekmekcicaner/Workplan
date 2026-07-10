namespace Workplan.SharedKernel.Common;

public class Result : IFailable<Result>
{
    protected Result(bool isSuccess, Error error)
    {
        switch (isSuccess)
        {
            case true when error != Error.None:
                throw new InvalidOperationException("Başarılı Result'ın Error'ı olamaz.");
            case false when error == Error.None:
                throw new InvalidOperationException("Başarısız Result bir Error taşımalı.");
            default:
                IsSuccess = isSuccess;
                Error = error;
                break;
        }
    }

    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public Error Error { get; }

    public static Result Ok() => new(true, Error.None);
    public static Result Fail(Error error) => new(false, error);
    public static Result Fail(string message) => new(false, Error.Validation(message));
}

public sealed class Result<T> : Result, IFailable<Result<T>>
{
    private readonly T? _value;

    private Result(bool isSuccess, T? value, Error error) : base(isSuccess, error)
        => _value = value;

    public T Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("Başarısız Result'ın Value'suna erişilemez.");

    public static Result<T> Ok(T value) => new(true, value, Error.None);
    public new static Result<T> Fail(Error error) => new(false, default, error);
    public new static Result<T> Fail(string message) => new(false, default, Error.Validation(message));

    // T döndüğünde otomatik olarak başarılı bir Result<T> oluşur
    public static implicit operator Result<T>(T value) => Ok(value);

    // Error döndüğünde otomatik olarak başarısız bir Result<T> oluşur
    public static implicit operator Result<T>(Error error) => Fail(error);
}