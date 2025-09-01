#nullable disable

using Microsoft.EntityFrameworkCore.Migrations;

namespace AtomicLmsCore.Infrastructure.Migrations.Solutions;

/// <inheritdoc />
public partial class InitialCreate : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            "Tenant",
            table => new
            {
                InternalId = table.Column<int>("int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                Name = table.Column<string>("nvarchar(255)", maxLength: 255, nullable: false),
                Slug = table.Column<string>("nvarchar(100)", maxLength: 100, nullable: false),
                IsActive = table.Column<bool>("bit", nullable: false, defaultValue: true),
                Metadata = table.Column<string>("nvarchar(max)", nullable: false),
                Id = table.Column<Guid>("uniqueidentifier", nullable: false),
                CreatedAt = table.Column<DateTime>("datetime2", nullable: false),
                UpdatedAt = table.Column<DateTime>("datetime2", nullable: false),
                CreatedBy = table.Column<string>("nvarchar(max)", nullable: false),
                UpdatedBy = table.Column<string>("nvarchar(max)", nullable: false),
                IsDeleted = table.Column<bool>("bit", nullable: false),
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Tenant", x => x.InternalId);
            });

        migrationBuilder.CreateIndex(
            "IX_Tenant_Id",
            "Tenant",
            "Id",
            unique: true);

        migrationBuilder.CreateIndex(
            "IX_Tenant_Slug",
            "Tenant",
            "Slug",
            unique: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
        => migrationBuilder.DropTable(name: "Tenant");
}
