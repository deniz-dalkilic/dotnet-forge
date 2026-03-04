namespace Template.Application.Abstractions;

public interface IRefreshTokenService
{
    Task<RefreshTokenResult> IssueAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<RefreshTokenResult?> RotateAsync(string oldToken, CancellationToken cancellationToken = default);
    Task<bool> RevokeAsync(string token, CancellationToken cancellationToken = default);
}

public sealed record RefreshTokenResult(Guid UserId, string Token, DateTimeOffset ExpiresAtUtc);
