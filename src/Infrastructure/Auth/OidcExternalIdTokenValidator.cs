using System.Collections.Concurrent;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using Template.Application.Abstractions;
using Template.Application.Auth;
using Template.Domain.Entities;

namespace Template.Infrastructure.Auth;

public sealed class OidcExternalIdTokenValidator(IOptions<ExternalAuthOptions> options) : IExternalIdTokenValidator
{
    private static readonly ConcurrentDictionary<ExternalAuthProvider, ConfigurationManager<OpenIdConnectConfiguration>> ConfigurationManagers = new();

    private readonly ExternalAuthOptions _options = options.Value;
    private readonly JwtSecurityTokenHandler _tokenHandler = new();

    public async Task<ExternalIdentityPayload> ValidateAsync(
        ExternalAuthProvider provider,
        string idToken,
        string? expectedNonce,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(idToken))
        {
            throw new ExternalAuthValidationException("External ID token is required.");
        }

        var providerConfig = GetProviderConfig(provider);
        var configuration = await providerConfig.ConfigurationManager.GetConfigurationAsync(cancellationToken);

        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKeys = configuration.SigningKeys,
            ValidateIssuer = true,
            ValidIssuers = configuration.Issuer is null
                ? providerConfig.ValidIssuers
                : providerConfig.ValidIssuers.Append(configuration.Issuer),
            ValidateAudience = true,
            ValidAudience = providerConfig.ClientId,
            AudienceValidator = (audiences, _, parameters) =>
            {
                if (parameters.ValidAudience is null)
                {
                    return false;
                }

                return audiences?.Any(audience =>
                    string.Equals(audience, parameters.ValidAudience, StringComparison.Ordinal)) == true;
            },
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(2)
        };

        try
        {
            var principal = _tokenHandler.ValidateToken(idToken, tokenValidationParameters, out var validatedToken);
            var jwt = validatedToken as JwtSecurityToken
                ?? throw new ExternalAuthValidationException("External ID token format is invalid.");

            ValidateNonce(principal, expectedNonce);

            var subject = GetClaimValue(principal, JwtRegisteredClaimNames.Sub)
                ?? throw new ExternalAuthValidationException("External ID token missing subject.");

            var audience = GetClaimValue(principal, JwtRegisteredClaimNames.Aud)
                ?? jwt.Audiences.FirstOrDefault()
                ?? providerConfig.ClientId;

            var email = GetClaimValue(principal, JwtRegisteredClaimNames.Email);
            var name = GetClaimValue(principal, JwtRegisteredClaimNames.Name)
                ?? GetClaimValue(principal, "preferred_username");
            var issuer = GetClaimValue(principal, JwtRegisteredClaimNames.Iss)
                ?? jwt.Issuer;
            var expiresAtUtc = DateTimeOffset.FromUnixTimeSeconds(jwt.Payload.Expiration ?? 0L);
            var emailVerified = ParseEmailVerified(principal.FindFirst("email_verified")?.Value);

            return new ExternalIdentityPayload(
                provider,
                subject,
                email,
                name,
                issuer,
                audience,
                expiresAtUtc,
                emailVerified);
        }
        catch (ExternalAuthValidationException)
        {
            throw;
        }
        catch (SecurityTokenException)
        {
            throw new ExternalAuthValidationException("External ID token validation failed.");
        }
        catch (ArgumentException)
        {
            throw new ExternalAuthValidationException("External ID token validation failed.");
        }
    }

    private static bool ParseEmailVerified(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        if (bool.TryParse(value, out var boolValue))
        {
            return boolValue;
        }

        return value == "1";
    }

    private static void ValidateNonce(ClaimsPrincipal principal, string? expectedNonce)
    {
        if (string.IsNullOrWhiteSpace(expectedNonce))
        {
            return;
        }

        var nonce = GetClaimValue(principal, "nonce");
        if (!string.Equals(nonce, expectedNonce, StringComparison.Ordinal))
        {
            throw new ExternalAuthValidationException("External ID token nonce mismatch.");
        }
    }


    private static string? GetClaimValue(ClaimsPrincipal principal, string claimType)
    {
        return principal.FindFirst(claimType)?.Value;
    }

    private ProviderConfiguration GetProviderConfig(ExternalAuthProvider provider)
    {
        return provider switch
        {
            ExternalAuthProvider.Google => new ProviderConfiguration(
                _options.Providers.Google.ClientId,
                GetOrCreateConfigurationManager(provider, _options.GoogleDiscoveryUrl),
                ["https://accounts.google.com", "accounts.google.com"]),

            ExternalAuthProvider.Microsoft => new ProviderConfiguration(
                _options.Providers.Microsoft.ClientId,
                GetOrCreateConfigurationManager(provider, GetMicrosoftDiscoveryUrl()),
                []),

            ExternalAuthProvider.Apple => new ProviderConfiguration(
                _options.Providers.Apple.ClientId,
                GetOrCreateConfigurationManager(provider, _options.AppleDiscoveryUrl),
                ["https://appleid.apple.com"]),

            _ => throw new ExternalAuthValidationException("Unsupported external auth provider.")
        };
    }

    private string GetMicrosoftDiscoveryUrl()
    {
        var tenant = string.IsNullOrWhiteSpace(_options.Providers.Microsoft.Tenant)
            ? "common"
            : _options.Providers.Microsoft.Tenant;

        return _options.MicrosoftDiscoveryUrlTemplate.Replace("{tenant}", tenant, StringComparison.OrdinalIgnoreCase);
    }

    private static ConfigurationManager<OpenIdConnectConfiguration> GetOrCreateConfigurationManager(
        ExternalAuthProvider provider,
        string discoveryUrl)
    {
        return ConfigurationManagers.GetOrAdd(provider, _ =>
            new ConfigurationManager<OpenIdConnectConfiguration>(
                discoveryUrl,
                new OpenIdConnectConfigurationRetriever(),
                new HttpDocumentRetriever { RequireHttps = true }));
    }

    private sealed record ProviderConfiguration(
        string ClientId,
        ConfigurationManager<OpenIdConnectConfiguration> ConfigurationManager,
        string[] ValidIssuers);
}
