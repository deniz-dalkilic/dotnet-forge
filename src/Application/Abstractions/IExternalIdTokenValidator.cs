using Template.Application.Auth;
using Template.Domain.Entities;

namespace Template.Application.Abstractions;

public interface IExternalIdTokenValidator
{
    Task<ExternalIdentityPayload> ValidateAsync(
        ExternalAuthProvider provider,
        string idToken,
        string? expectedNonce,
        CancellationToken cancellationToken);
}
