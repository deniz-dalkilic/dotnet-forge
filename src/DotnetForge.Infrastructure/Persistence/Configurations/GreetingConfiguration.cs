using DotnetForge.Domain.Greetings;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DotnetForge.Infrastructure.Persistence.Configurations;

public sealed class GreetingConfiguration : IEntityTypeConfiguration<Greeting>
{
    public void Configure(EntityTypeBuilder<Greeting> builder)
    {
        builder.ToTable("greetings");

        builder.HasKey(greeting => greeting.Id);

        builder.Property(greeting => greeting.Id)
            .ValueGeneratedNever();

        builder.Property(greeting => greeting.Name)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(greeting => greeting.Message)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(greeting => greeting.CreatedAtUtc)
            .IsRequired();

        builder.HasIndex(greeting => greeting.CreatedAtUtc);
    }
}
