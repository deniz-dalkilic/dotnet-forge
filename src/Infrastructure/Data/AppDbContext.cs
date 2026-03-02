using Microsoft.EntityFrameworkCore;
using Template.Application.Abstractions;
using Template.Domain.Entities;
using Template.Infrastructure.Data.Configurations;

namespace Template.Infrastructure.Data;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options)
    : DbContext(options), IAppDbContext
{
    public DbSet<SampleItem> SampleItems => Set<SampleItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
