using DotnetForge.Infrastructure.Observability;
using DotnetForge.Worker.Extensions;

var builder = Host.CreateApplicationBuilder(args);

builder.AddForgeObservability("DotnetForge.Worker", "1.0.0", includeAspNetCoreInstrumentation: false);
builder.Services.AddWorkerServices(builder.Configuration);

var host = builder.Build();

host.Run();
