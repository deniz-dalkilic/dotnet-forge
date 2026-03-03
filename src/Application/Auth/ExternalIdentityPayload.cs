using Template.Domain.Entities;

namespace Template.Application.Auth;

public sealed record ExternalIdentityPayload(
    ExternalAuthProvider Provider,
    string Subject,
    string? Email,
    string? Name,
    string Issuer,
    string Audience,
    DateTimeOffset ExpiresAtUtc,
    bool EmailVerified = false);
