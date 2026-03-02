using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Template.Domain.Entities;

namespace Template.Infrastructure.Data.Configurations;

public sealed class SampleItemConfiguration : IEntityTypeConfiguration<SampleItem>
{
    public void Configure(EntityTypeBuilder<SampleItem> builder)
    {
        builder.ToTable("sample_items");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).HasMaxLength(256).IsRequired();
        builder.Property(x => x.CreatedAtUtc).IsRequired();
    }
}
