using Quartz;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using Template.Infrastructure.DependencyInjection;
using Template.Worker.Messaging;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource.AddService("template-worker"))
    .WithTracing(tracing => tracing
        .AddHttpClientInstrumentation()
        .AddOtlpExporter());

builder.Services.AddQuartzHostedService(options => options.WaitForJobsToComplete = true);
builder.Services.AddHostedService<SampleItemCreatedConsumerService>();

builder.Services.AddSerilog(config => config.WriteTo.Console());

var host = builder.Build();
await host.RunAsync();
