namespace Template.Infrastructure.Auth;

public sealed class ExternalAuthValidationException(string message) : Exception(message);
