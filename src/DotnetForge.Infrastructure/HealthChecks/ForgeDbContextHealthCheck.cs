using DotnetForge.Infrastructure.Persistence;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace DotnetForge.Infrastructure.HealthChecks;

public sealed class ForgeDbContextHealthCheck : IHealthCheck
{
    private readonly ForgeDbContext _dbContext;

    public ForgeDbContextHealthCheck(ForgeDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var canConnect = await _dbContext.Database.CanConnectAsync(cancellationToken);

        return canConnect
            ? HealthCheckResult.Healthy("PostgreSQL connection succeeded.")
            : HealthCheckResult.Unhealthy("PostgreSQL connection failed.");
    }
}
