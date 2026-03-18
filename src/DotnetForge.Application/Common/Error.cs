namespace DotnetForge.Application.Common;

public sealed record Error(string Code, string Message)
{
    public static Error Validation(string message) => new("validation", message);

    public static Error Unexpected(string message) => new("unexpected", message);
}
