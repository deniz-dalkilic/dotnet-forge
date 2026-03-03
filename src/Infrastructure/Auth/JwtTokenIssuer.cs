using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Template.Application.Abstractions;
using Template.Domain.Entities;

namespace Template.Infrastructure.Auth;

public sealed class JwtTokenIssuer(IOptions<JwtOptions> options) : IJwtTokenIssuer
{
    private readonly JwtOptions _options = options.Value;

    public string IssueAccessToken(
        User user,
        IEnumerable<string> roles,
        TimeSpan lifetime,
        IReadOnlyDictionary<string, string>? extraClaims = null)
    {
        ArgumentNullException.ThrowIfNull(user);
        ArgumentNullException.ThrowIfNull(roles);

        var signingKeyBytes = Encoding.UTF8.GetBytes(_options.SigningKey);
        if (signingKeyBytes.Length < 32)
        {
            throw new InvalidOperationException("Jwt:SigningKey must be at least 32 bytes.");
        }

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Name, user.DisplayName)
        };

        if (!string.IsNullOrWhiteSpace(user.Email))
        {
            claims.Add(new Claim(JwtRegisteredClaimNames.Email, user.Email));
        }

        foreach (var role in roles.Where(static x => !string.IsNullOrWhiteSpace(x)).Distinct(StringComparer.Ordinal))
        {
            claims.Add(new Claim("roles", role));
        }

        if (extraClaims is not null)
        {
            foreach (var claim in extraClaims.Where(static kv =>
                         !string.IsNullOrWhiteSpace(kv.Key) &&
                         !string.IsNullOrWhiteSpace(kv.Value)))
            {
                claims.Add(new Claim(claim.Key, claim.Value));
            }
        }

        var now = DateTime.UtcNow;
        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            notBefore: now,
            expires: now.Add(lifetime),
            signingCredentials: new SigningCredentials(new SymmetricSecurityKey(signingKeyBytes), SecurityAlgorithms.HmacSha256));

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
