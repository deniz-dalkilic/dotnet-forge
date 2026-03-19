using DotnetForge.Domain.Greetings;
using Microsoft.EntityFrameworkCore;

namespace DotnetForge.Infrastructure.Persistence;

public sealed class ForgeDbContext : DbContext
{
    public ForgeDbContext(DbContextOptions<ForgeDbContext> options)
        : base(options)
    {
    }

    public DbSet<Greeting> Greetings => Set<Greeting>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("forge");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(IInfrastructureAssemblyMarker).Assembly);
    }
}
