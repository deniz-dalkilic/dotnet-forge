using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using Serilog.Events;

namespace DotnetForge.Infrastructure.Observability;

public static class HostApplicationBuilderExtensions
{
    public static IHostApplicationBuilder AddForgeObservability(
        this IHostApplicationBuilder builder,
        string serviceName,
        string serviceVersion,
        bool includeAspNetCoreInstrumentation)
    {
        builder.Services.AddOptions<ObservabilityOptions>()
            .Bind(builder.Configuration.GetSection(ObservabilityOptions.SectionName))
            .Validate(options => !options.Seq.Enabled || Uri.TryCreate(options.Seq.ServerUrl, UriKind.Absolute, out _),
                $"{ObservabilityOptions.SectionName}:Seq:ServerUrl must be an absolute URI when Seq is enabled.")
            .ValidateOnStart();

        builder.Logging.ClearProviders();
        builder.Services.AddSerilog((services, loggerConfiguration) =>
        {
            var observabilityOptions = services.GetRequiredService<IOptions<ObservabilityOptions>>().Value;

            loggerConfiguration
                .MinimumLevel.Information()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.EntityFrameworkCore.Database.Command", LogEventLevel.Warning)
                .Enrich.FromLogContext()
                .Enrich.With(new ActivityTraceEnricher())
                .Enrich.WithMachineName()
                .Enrich.WithEnvironmentName()
                .Enrich.WithThreadId()
                .Enrich.WithProcessId()
                .Enrich.WithProperty("ServiceName", serviceName)
                .Enrich.WithProperty("ServiceVersion", serviceVersion)
                .WriteTo.Console();

            if (observabilityOptions.Seq.Enabled && !string.IsNullOrWhiteSpace(observabilityOptions.Seq.ServerUrl))
            {
                loggerConfiguration.WriteTo.Seq(observabilityOptions.Seq.ServerUrl, apiKey: observabilityOptions.Seq.ApiKey);
            }
        });

        var openTelemetryEnabled = builder.Configuration.GetValue<bool?>("Observability:OpenTelemetry:Enabled") ?? true;
        if (openTelemetryEnabled)
        {
            var openTelemetry = builder.Services.AddOpenTelemetry();
            openTelemetry.ConfigureResource(resource => resource.AddService(
                serviceName: serviceName,
                serviceVersion: serviceVersion,
                serviceNamespace: "DotnetForge"));

            openTelemetry.WithTracing(tracing =>
            {
                tracing.AddSource(ForgeTelemetry.ActivitySourceName)
                    .AddHttpClientInstrumentation();

                if (includeAspNetCoreInstrumentation)
                {
                    tracing.AddAspNetCoreInstrumentation(options =>
                    {
                        options.RecordException = true;
                    });
                }

                tracing.AddConditionalOtlpExporter(builder.Configuration);
            });

            openTelemetry.WithMetrics(metrics =>
            {
                metrics.AddMeter(ForgeTelemetry.MeterName)
                    .AddHttpClientInstrumentation()
                    .AddRuntimeInstrumentation();

                if (includeAspNetCoreInstrumentation)
                {
                    metrics.AddAspNetCoreInstrumentation();
                }

                metrics.AddConditionalOtlpExporter(builder.Configuration);
            });
        }

        builder.Services.AddSingleton<IHostedService>(serviceProvider =>
            new ObservabilityHostedService(
                serviceName,
                serviceProvider.GetRequiredService<IOptions<ObservabilityOptions>>(),
                serviceProvider.GetRequiredService<ILogger<ObservabilityHostedService>>()));

        return builder;
    }

    private static TracerProviderBuilder AddConditionalOtlpExporter(this TracerProviderBuilder tracing, IConfiguration configuration)
    {
        var options = configuration.GetSection($"{ObservabilityOptions.SectionName}:OpenTelemetry:Otlp").Get<OtlpExporterOptions>();
        if (options is null || !options.Enabled || string.IsNullOrWhiteSpace(options.Endpoint))
        {
            return tracing;
        }

        tracing.AddOtlpExporter(exporterOptions =>
        {
            exporterOptions.Endpoint = new Uri(options.Endpoint);
            exporterOptions.Protocol = options.Protocol.Equals("http/protobuf", StringComparison.OrdinalIgnoreCase)
                ? OpenTelemetry.Exporter.OtlpExportProtocol.HttpProtobuf
                : OpenTelemetry.Exporter.OtlpExportProtocol.Grpc;

            if (options.Headers is { Count: > 0 })
            {
                exporterOptions.Headers = string.Join(",", options.Headers.Select(pair => $"{pair.Key}={pair.Value}"));
            }
        });

        return tracing;
    }

    private static MeterProviderBuilder AddConditionalOtlpExporter(this MeterProviderBuilder metrics, IConfiguration configuration)
    {
        var options = configuration.GetSection($"{ObservabilityOptions.SectionName}:OpenTelemetry:Otlp").Get<OtlpExporterOptions>();
        if (options is null || !options.Enabled || string.IsNullOrWhiteSpace(options.Endpoint))
        {
            return metrics;
        }

        metrics.AddOtlpExporter(exporterOptions =>
        {
            exporterOptions.Endpoint = new Uri(options.Endpoint);
            exporterOptions.Protocol = options.Protocol.Equals("http/protobuf", StringComparison.OrdinalIgnoreCase)
                ? OpenTelemetry.Exporter.OtlpExportProtocol.HttpProtobuf
                : OpenTelemetry.Exporter.OtlpExportProtocol.Grpc;

            if (options.Headers is { Count: > 0 })
            {
                exporterOptions.Headers = string.Join(",", options.Headers.Select(pair => $"{pair.Key}={pair.Value}"));
            }
        });

        return metrics;
    }

    private sealed class ObservabilityHostedService : IHostedService
    {
        private readonly string _serviceName;
        private readonly ObservabilityOptions _options;
        private readonly ILogger<ObservabilityHostedService> _logger;

        public ObservabilityHostedService(
            string serviceName,
            IOptions<ObservabilityOptions> options,
            ILogger<ObservabilityHostedService> logger)
        {
            _serviceName = serviceName;
            _options = options.Value;
            _logger = logger;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation(
                "Observability configured for {ServiceName}. SeqEnabled={SeqEnabled} OtlpEnabled={OtlpEnabled} SensitiveHeaders={SensitiveHeaders}",
                _serviceName,
                _options.Seq.Enabled,
                _options.OpenTelemetry.Otlp.Enabled,
                string.Join(", ", _options.Redaction.SensitiveHeaders));

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
