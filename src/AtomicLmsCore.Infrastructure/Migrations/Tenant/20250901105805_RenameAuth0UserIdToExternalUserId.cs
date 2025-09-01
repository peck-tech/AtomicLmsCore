#nullable disable

using Microsoft.EntityFrameworkCore.Migrations;

namespace AtomicLmsCore.Infrastructure.Migrations.Tenant;

/// <inheritdoc />
public partial class RenameAuth0UserIdToExternalUserId : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.RenameColumn(
            "Auth0UserId",
            "User",
            "ExternalUserId");

        migrationBuilder.RenameIndex(
            "IX_User_Auth0UserId",
            table: "User",
            newName: "IX_User_ExternalUserId");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.RenameColumn(
            "ExternalUserId",
            "User",
            "Auth0UserId");

        migrationBuilder.RenameIndex(
            "IX_User_ExternalUserId",
            table: "User",
            newName: "IX_User_Auth0UserId");
    }
}
