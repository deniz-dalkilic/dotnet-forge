namespace DotnetForge.Application.Common;

public class Result
{
    protected Result(bool isSuccess, Error? error, IReadOnlyDictionary<string, string[]>? validationErrors = null)
    {
        IsSuccess = isSuccess;
        Error = error;
        ValidationErrors = validationErrors;
    }

    public bool IsSuccess { get; }

    public bool IsFailure => !IsSuccess;

    public Error? Error { get; }

    public IReadOnlyDictionary<string, string[]>? ValidationErrors { get; }

    public static Result Success() => new(true, null);

    public static Result Failure(Error error) => new(false, error);

    public static Result ValidationFailure(IReadOnlyDictionary<string, string[]> errors)
        => new(false, Error.Validation("One or more validation errors occurred."), errors);
}

public sealed class Result<T> : Result
{
    private Result(T? value, bool isSuccess, Error? error, IReadOnlyDictionary<string, string[]>? validationErrors = null)
        : base(isSuccess, error, validationErrors)
    {
        Value = value;
    }

    public T? Value { get; }

    public static Result<T> Success(T value) => new(value, true, null);

    public static new Result<T> Failure(Error error) => new(default, false, error);

    public static Result<T> ValidationFailure(IReadOnlyDictionary<string, string[]> errors)
        => new(default, false, Error.Validation("One or more validation errors occurred."), errors);
}
