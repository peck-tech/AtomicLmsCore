using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AtomicLmsCore.Infrastructure.Migrations.Solutions;

/// <inheritdoc />
public partial class AddDatabaseNameToTenant : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "DatabaseName",
            table: "Tenant",
            type: "nvarchar(255)",
            maxLength: 255,
            nullable: false,
            defaultValue: string.Empty);

        migrationBuilder.CreateIndex(
            name: "IX_Tenant_DatabaseName",
            table: "Tenant",
            column: "DatabaseName",
            unique: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_Tenant_DatabaseName",
            table: "Tenant");

        migrationBuilder.DropColumn(
            name: "DatabaseName",
            table: "Tenant");
    }
}
