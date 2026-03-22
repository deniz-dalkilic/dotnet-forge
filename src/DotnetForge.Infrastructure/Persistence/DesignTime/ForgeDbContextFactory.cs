using DotnetForge.Infrastructure.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace DotnetForge.Infrastructure.Persistence.DesignTime;

public sealed class ForgeDbContextFactory : IDesignTimeDbContextFactory<ForgeDbContext>
{
    public ForgeDbContext CreateDbContext(string[] args)
    {
        var connectionString =
            Environment.GetEnvironmentVariable("Database__ConnectionString") ??
            "Host=localhost;Port=5432;Database=appdb;Username=appuser;Password=apppassword";

        var optionsBuilder = new DbContextOptionsBuilder<ForgeDbContext>();
        optionsBuilder.UseNpgsql(connectionString, npgsql =>
        {
            npgsql.MigrationsHistoryTable("__ef_migrations_history", "forge");
        });

        return new ForgeDbContext(optionsBuilder.Options);
    }
}
