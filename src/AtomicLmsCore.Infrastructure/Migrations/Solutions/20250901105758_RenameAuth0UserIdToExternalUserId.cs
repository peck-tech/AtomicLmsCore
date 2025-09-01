using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AtomicLmsCore.Infrastructure.Migrations.Solutions;

/// <inheritdoc />
public partial class RenameAuth0UserIdToExternalUserId : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.RenameColumn(
            name: "Auth0UserId",
            table: "User",
            newName: "ExternalUserId");

        migrationBuilder.RenameIndex(
            name: "IX_User_Auth0UserId",
            table: "User",
            newName: "IX_User_ExternalUserId");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.RenameColumn(
            name: "ExternalUserId",
            table: "User",
            newName: "Auth0UserId");

        migrationBuilder.RenameIndex(
            name: "IX_User_ExternalUserId",
            table: "User",
            newName: "IX_User_Auth0UserId");
    }
}
