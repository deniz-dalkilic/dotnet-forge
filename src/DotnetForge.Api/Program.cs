using DotnetForge.Api.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApiServices(builder.Configuration);

var app = builder.Build();

app.Logger.LogInformation("Starting {ApplicationName} in {EnvironmentName}",
    app.Environment.ApplicationName,
    app.Environment.EnvironmentName);

await app.UseApiPipelineAsync();

app.Logger.LogInformation("{ApplicationName} started and endpoints registered", app.Environment.ApplicationName);

app.Run();

public partial class Program;
