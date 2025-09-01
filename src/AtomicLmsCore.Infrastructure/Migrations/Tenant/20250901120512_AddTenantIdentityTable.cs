#nullable disable

using Microsoft.EntityFrameworkCore.Migrations;

namespace AtomicLmsCore.Infrastructure.Migrations.Tenant;

/// <inheritdoc />
public partial class AddTenantIdentityTable : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            "__tenant_identity",
            table => new
            {
                TenantId = table.Column<Guid>("uniqueidentifier", nullable: false),
                DatabaseName = table.Column<string>("nvarchar(255)", maxLength: 255, nullable: false),
                CreatedAt = table.Column<DateTime>("datetime2", nullable: false),
                ValidationHash = table.Column<string>("nvarchar(128)", maxLength: 128, nullable: false),
                CreationMetadata = table.Column<string>("nvarchar(1000)", maxLength: 1000, nullable: false, defaultValue: string.Empty),
            },
            constraints: table =>
            {
                table.PrimaryKey("PK___tenant_identity", x => x.TenantId);
            });

        migrationBuilder.CreateIndex(
            "IX___tenant_identity_DatabaseName",
            "__tenant_identity",
            "DatabaseName");

        migrationBuilder.CreateIndex(
            "IX___tenant_identity_ValidationHash",
            "__tenant_identity",
            "ValidationHash");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
        => migrationBuilder.DropTable(name: "__tenant_identity");
}
