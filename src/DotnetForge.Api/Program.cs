var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

app.MapGet("/", () => Results.Ok(new { Service = "DotnetForge.Api", Status = "BootstrapReady" }));

app.Run();
