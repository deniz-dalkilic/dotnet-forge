using System.Reflection;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Quartz;
using Serilog;
using Serilog.Sinks.Grafana.Loki;
using Template.Infrastructure.Configuration;
using Template.Infrastructure.DependencyInjection;
using Template.Infrastructure.Jobs;
using Template.Infrastructure.Messaging;
using Template.Worker.Messaging;
using Template.Worker.Observability;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateBootstrapLogger();

DotEnvLoader.LoadFromContentRoot(Directory.GetCurrentDirectory());

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.Configure<CleanupOldSampleItemsOptions>(
    builder.Configuration.GetSection(CleanupOldSampleItemsOptions.SectionName));

var serviceName = builder.Configuration["OpenTelemetry:ServiceName"] ?? "template-worker";
var serviceVersion = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "unknown";
var environmentName = builder.Environment.EnvironmentName;
var otlpEndpoint = builder.Configuration["OpenTelemetry:OtlpEndpoint"];

builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource.AddService(serviceName, serviceVersion: serviceVersion)
        .AddAttributes(new Dictionary<string, object>
        {
            ["deployment.environment"] = environmentName
        }))
    .WithTracing(tracing =>
    {
        tracing
            .AddSource(MessagingActivitySource.Name)
            .AddHttpClientInstrumentation();

        if (!string.IsNullOrWhiteSpace(otlpEndpoint))
        {
            tracing.AddOtlpExporter(options => options.Endpoint = new Uri(otlpEndpoint));
        }
    })
    .WithMetrics(metrics =>
    {
        metrics
            .AddRuntimeInstrumentation()
            .AddProcessInstrumentation();

        if (!string.IsNullOrWhiteSpace(otlpEndpoint))
        {
            metrics.AddOtlpExporter(options => options.Endpoint = new Uri(otlpEndpoint));
        }
    });

builder.Services.AddQuartz(q =>
{
    var options = builder.Configuration
        .GetSection(CleanupOldSampleItemsOptions.SectionName)
        .Get<CleanupOldSampleItemsOptions>() ?? new CleanupOldSampleItemsOptions();

    var jobKey = new JobKey(nameof(CleanupOldSampleItemsJob));

    q.AddJob<CleanupOldSampleItemsJob>(job => job.WithIdentity(jobKey));
    q.AddTrigger(trigger => trigger
        .ForJob(jobKey)
        .WithIdentity($"{nameof(CleanupOldSampleItemsJob)}-trigger")
        .WithCronSchedule(options.Cron));
});

builder.Services.AddQuartzHostedService(options => options.WaitForJobsToComplete = true);
builder.Services.AddHostedService<SampleItemCreatedConsumerService>();

builder.Services.AddSerilog((services, configuration) =>
{
    configuration
        .ReadFrom.Configuration(builder.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .Enrich.WithEnvironmentName()
        .Enrich.WithThreadId()
        .Enrich.With(new TraceContextEnricher())
        .WriteTo.Console();

    var lokiEndpoint = builder.Configuration["Serilog:Loki:Endpoint"];
    if (!string.IsNullOrWhiteSpace(lokiEndpoint))
    {
        configuration.WriteTo.GrafanaLoki(lokiEndpoint);
    }
});

var host = builder.Build();
await host.RunAsync();
