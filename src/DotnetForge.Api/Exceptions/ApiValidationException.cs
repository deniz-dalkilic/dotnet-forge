namespace DotnetForge.Api.Exceptions;

public sealed class ApiValidationException : Exception
{
    public ApiValidationException(IReadOnlyDictionary<string, string[]> errors, string? message = null)
        : base(message ?? "Validation failed.")
    {
        Errors = errors;
    }

    public IReadOnlyDictionary<string, string[]> Errors { get; }
}
