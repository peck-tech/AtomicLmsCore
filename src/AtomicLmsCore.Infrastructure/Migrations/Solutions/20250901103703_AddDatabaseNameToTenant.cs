#nullable disable

using Microsoft.EntityFrameworkCore.Migrations;

namespace AtomicLmsCore.Infrastructure.Migrations.Solutions;

/// <inheritdoc />
public partial class AddDatabaseNameToTenant : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            "DatabaseName",
            "Tenant",
            "nvarchar(255)",
            maxLength: 255,
            nullable: false,
            defaultValue: string.Empty);

        migrationBuilder.CreateIndex(
            "IX_Tenant_DatabaseName",
            "Tenant",
            "DatabaseName",
            unique: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            "IX_Tenant_DatabaseName",
            "Tenant");

        migrationBuilder.DropColumn(
            "DatabaseName",
            "Tenant");
    }
}
