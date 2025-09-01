using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AtomicLmsCore.Infrastructure.Migrations.Solutions;

/// <inheritdoc />
public partial class InitialCreate : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Tenant",
            columns: table => new
            {
                InternalId = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                Slug = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                Metadata = table.Column<string>(type: "nvarchar(max)", nullable: false),
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                IsDeleted = table.Column<bool>(type: "bit", nullable: false),
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Tenant", x => x.InternalId);
            });

        migrationBuilder.CreateIndex(
            name: "IX_Tenant_Id",
            table: "Tenant",
            column: "Id",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_Tenant_Slug",
            table: "Tenant",
            column: "Slug",
            unique: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "Tenant");
    }
}
