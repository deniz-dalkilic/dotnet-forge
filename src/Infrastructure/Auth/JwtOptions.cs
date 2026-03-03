using System.ComponentModel.DataAnnotations;

namespace Template.Infrastructure.Auth;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    [Required]
    public string Issuer { get; init; } = string.Empty;

    [Required]
    public string Audience { get; init; } = string.Empty;

    [Required]
    public string SigningKey { get; init; } = string.Empty;

    [Range(1, 1440)]
    public int LifetimeMinutes { get; init; } = 15;
}
