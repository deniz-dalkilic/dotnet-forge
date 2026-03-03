using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using Template.Infrastructure.Data;

#nullable disable

namespace Template.Infrastructure.Data.Migrations;

[DbContext(typeof(AppDbContext))]
partial class AppDbContextModelSnapshot : ModelSnapshot
{
    protected override void BuildModel(ModelBuilder modelBuilder)
    {
#pragma warning disable 612, 618
        modelBuilder
            .HasAnnotation("ProductVersion", "9.0.0")
            .HasAnnotation("Relational:MaxIdentifierLength", 63);

        NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

        modelBuilder.Entity("Template.Domain.Entities.ExternalIdentity", b =>
            {
                b.Property<Guid>("Id")
                    .ValueGeneratedOnAdd()
                    .HasColumnType("uuid");

                b.Property<string>("Email")
                    .HasMaxLength(320)
                    .HasColumnType("character varying(320)");

                b.Property<DateTimeOffset>("LinkedAtUtc")
                    .HasColumnType("timestamp with time zone");

                b.Property<short>("Provider")
                    .HasColumnType("smallint");

                b.Property<string>("Subject")
                    .IsRequired()
                    .HasMaxLength(512)
                    .HasColumnType("character varying(512)");

                b.Property<Guid>("UserId")
                    .HasColumnType("uuid");

                b.HasKey("Id");

                b.HasIndex("Provider", "Subject")
                    .IsUnique();

                b.HasIndex("UserId");

                b.ToTable("external_identities", (string)null);
            });

        modelBuilder.Entity("Template.Domain.Entities.SampleItem", b =>
            {
                b.Property<Guid>("Id")
                    .ValueGeneratedOnAdd()
                    .HasColumnType("uuid");

                b.Property<DateTime>("CreatedAtUtc")
                    .HasColumnType("timestamp with time zone");

                b.Property<string>("Name")
                    .IsRequired()
                    .HasMaxLength(256)
                    .HasColumnType("character varying(256)");

                b.HasKey("Id");

                b.ToTable("sample_items", (string)null);
            });

        modelBuilder.Entity("Template.Domain.Entities.User", b =>
            {
                b.Property<Guid>("Id")
                    .ValueGeneratedOnAdd()
                    .HasColumnType("uuid");

                b.Property<DateTimeOffset>("CreatedAtUtc")
                    .HasColumnType("timestamp with time zone");

                b.Property<string>("DisplayName")
                    .IsRequired()
                    .HasMaxLength(256)
                    .HasColumnType("character varying(256)");

                b.Property<string>("Email")
                    .HasMaxLength(320)
                    .HasColumnType("character varying(320)");

                b.HasKey("Id");

                b.HasIndex("Email");

                b.ToTable("users", (string)null);
            });

        modelBuilder.Entity("Template.Domain.Entities.ExternalIdentity", b =>
            {
                b.HasOne("Template.Domain.Entities.User", "User")
                    .WithMany()
                    .HasForeignKey("UserId")
                    .OnDelete(DeleteBehavior.Cascade)
                    .IsRequired();

                b.Navigation("User");
            });
#pragma warning restore 612, 618
    }
}
