using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DotnetForge.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialPersistence : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "forge");

            migrationBuilder.CreateTable(
                name: "greetings",
                schema: "forge",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Message = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_greetings", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_greetings_CreatedAtUtc",
                schema: "forge",
                table: "greetings",
                column: "CreatedAtUtc");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "greetings",
                schema: "forge");
        }
    }
}
