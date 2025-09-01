using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AtomicLmsCore.Infrastructure.Migrations.Tenant;

/// <inheritdoc />
public partial class AddTenantIdentityTable : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "__tenant_identity",
            columns: table => new
            {
                TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                DatabaseName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                ValidationHash = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                CreationMetadata = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false, defaultValue: string.Empty),
            },
            constraints: table =>
            {
                table.PrimaryKey("PK___tenant_identity", x => x.TenantId);
            });

        migrationBuilder.CreateIndex(
            name: "IX___tenant_identity_DatabaseName",
            table: "__tenant_identity",
            column: "DatabaseName");

        migrationBuilder.CreateIndex(
            name: "IX___tenant_identity_ValidationHash",
            table: "__tenant_identity",
            column: "ValidationHash");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "__tenant_identity");
    }
}
