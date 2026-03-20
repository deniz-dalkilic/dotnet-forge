namespace DotnetForge.Infrastructure.Observability;

public sealed class ObservabilityOptions
{
    public const string SectionName = "Observability";

    public SeqOptions Seq { get; init; } = new();

    public OpenTelemetryOptions OpenTelemetry { get; init; } = new();

    public RedactionOptions Redaction { get; init; } = new();
}

public sealed class SeqOptions
{
    public bool Enabled { get; init; } = true;

    public string ServerUrl { get; init; } = "http://localhost:5341";

    public string? ApiKey { get; init; }
}

public sealed class OpenTelemetryOptions
{
    public bool Enabled { get; init; } = true;

    public OtlpExporterOptions Otlp { get; init; } = new();
}

public sealed class OtlpExporterOptions
{
    public bool Enabled { get; init; }

    public string? Endpoint { get; init; }

    public string Protocol { get; init; } = "grpc";

    public Dictionary<string, string>? Headers { get; init; }
}

public sealed class RedactionOptions
{
    public string[] SensitiveHeaders { get; init; } = ["Authorization", "Cookie", "Set-Cookie", "X-Api-Key"];
}
