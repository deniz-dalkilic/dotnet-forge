using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Template.Application.Abstractions;
using Template.Application.Auth;
using Template.Domain.Entities;

namespace Template.Infrastructure.Auth;

public sealed class RefreshTokenService(
    IAppDbContext dbContext,
    IClock clock,
    IOptions<RefreshTokenOptions> options) : IRefreshTokenService
{
    public async Task<RefreshTokenResult> IssueAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var now = clock.UtcNow;
        var rawToken = GenerateToken();
        var tokenHash = Sha256TokenHasher.Hash(rawToken);
        var expiresAtUtc = now.AddDays(options.Value.LifetimeDays);

        dbContext.RefreshTokens.Add(new RefreshToken(userId, tokenHash, expiresAtUtc, now));
        await dbContext.SaveChangesAsync(cancellationToken);

        return new RefreshTokenResult(userId, rawToken, expiresAtUtc);
    }

    public async Task<RefreshTokenResult?> RotateAsync(string oldToken, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(oldToken);

        var now = clock.UtcNow;
        var oldHash = Sha256TokenHasher.Hash(oldToken);
        var current = await dbContext.RefreshTokens.FirstOrDefaultAsync(x => x.TokenHash == oldHash, cancellationToken);
        if (current is null || !current.IsActive(now))
        {
            return null;
        }

        var newRawToken = GenerateToken();
        var newHash = Sha256TokenHasher.Hash(newRawToken);
        var expiresAtUtc = now.AddDays(options.Value.LifetimeDays);

        current.Revoke(now, newHash);
        dbContext.RefreshTokens.Add(new RefreshToken(current.UserId, newHash, expiresAtUtc, now));
        await dbContext.SaveChangesAsync(cancellationToken);

        return new RefreshTokenResult(current.UserId, newRawToken, expiresAtUtc);
    }

    public async Task<bool> RevokeAsync(string token, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(token);

        var now = clock.UtcNow;
        var tokenHash = Sha256TokenHasher.Hash(token);
        var current = await dbContext.RefreshTokens.FirstOrDefaultAsync(x => x.TokenHash == tokenHash, cancellationToken);
        if (current is null || !current.IsActive(now))
        {
            return false;
        }

        current.Revoke(now);
        await dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    private static string GenerateToken()
    {
        Span<byte> bytes = stackalloc byte[32];
        RandomNumberGenerator.Fill(bytes);
        return Convert.ToBase64String(bytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }
}
