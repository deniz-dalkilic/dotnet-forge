namespace DotnetForge.Infrastructure.Options;

public sealed class DatabaseOptions
{
    public const string SectionName = "Database";

    public string ConnectionString { get; init; } = string.Empty;

    public bool EnableSensitiveDataLogging { get; init; }

    public bool ApplyMigrationsOnStartup { get; init; } = true;
}
