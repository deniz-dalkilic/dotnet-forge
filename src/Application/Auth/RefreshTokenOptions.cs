namespace Template.Application.Auth;

public sealed class RefreshTokenOptions
{
    public const string SectionName = "RefreshTokens";

    public bool Enabled { get; init; }
    public int LifetimeDays { get; init; } = 14;
}
