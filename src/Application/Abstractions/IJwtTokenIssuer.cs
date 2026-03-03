using Template.Domain.Entities;

namespace Template.Application.Abstractions;

public interface IJwtTokenIssuer
{
    string IssueAccessToken(
        User user,
        IEnumerable<string> roles,
        TimeSpan lifetime,
        IReadOnlyDictionary<string, string>? extraClaims = null);
}
