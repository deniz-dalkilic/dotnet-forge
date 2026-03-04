using Microsoft.Extensions.Options;
using Template.Application.Abstractions;
using Template.Domain.Entities;

namespace Template.Application.Auth;

public sealed class ExternalSignInService(
    IExternalIdTokenValidator idTokenValidator,
    IExternalIdentityRepository externalIdentityRepository,
    IUserRepository userRepository,
    IRefreshTokenService refreshTokenService,
    IOptions<RefreshTokenOptions> refreshTokenOptions,
    IUnitOfWork unitOfWork)
{
    public const string DefaultRole = "template-user";

    public async Task<ExternalSignInResult> SignInAsync(
        ExternalAuthProvider provider,
        string idToken,
        string? expectedNonce,
        CancellationToken cancellationToken)
    {
        var payload = await idTokenValidator.ValidateAsync(provider, idToken, expectedNonce, cancellationToken);

        var externalIdentity = await externalIdentityRepository.FindAsync(provider, payload.Subject, cancellationToken);
        if (externalIdentity is not null)
        {
            var existingUser = await userRepository.GetByIdAsync(externalIdentity.UserId, cancellationToken)
                ?? throw new InvalidOperationException("External identity is linked to a missing user.");

            var existingRefreshToken = await IssueRefreshTokenAsync(existingUser.Id, cancellationToken);
            return new ExternalSignInResult(existingUser, [DefaultRole], payload, existingRefreshToken);
        }

        // Intentionally does not auto-link by email to prevent account takeover scenarios.
        var user = new User(BuildDisplayName(payload), payload.Email);
        userRepository.Add(user);

        var identity = new ExternalIdentity(user.Id, provider, payload.Subject, payload.Email);
        externalIdentityRepository.Add(identity);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        var refreshToken = await IssueRefreshTokenAsync(user.Id, cancellationToken);
        return new ExternalSignInResult(user, [DefaultRole], payload, refreshToken);
    }

    private async Task<RefreshTokenResult?> IssueRefreshTokenAsync(Guid userId, CancellationToken cancellationToken)
    {
        if (!refreshTokenOptions.Value.Enabled)
        {
            return null;
        }

        return await refreshTokenService.IssueAsync(userId, cancellationToken);
    }

    private static string BuildDisplayName(ExternalIdentityPayload payload)
    {
        if (!string.IsNullOrWhiteSpace(payload.Name))
        {
            return payload.Name;
        }

        if (!string.IsNullOrWhiteSpace(payload.Email))
        {
            var atIndex = payload.Email.IndexOf('@');
            return atIndex > 0 ? payload.Email[..atIndex] : payload.Email;
        }

        return $"user-{payload.Subject}";
    }
}

public sealed record ExternalSignInResult(
    User User,
    IReadOnlyList<string> Roles,
    ExternalIdentityPayload Payload,
    RefreshTokenResult? RefreshToken);
