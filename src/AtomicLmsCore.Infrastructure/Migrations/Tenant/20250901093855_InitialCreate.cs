using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AtomicLmsCore.Infrastructure.Migrations.Tenant;

/// <inheritdoc />
public partial class InitialCreate : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "User",
            columns: table => new
            {
                InternalId = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                Auth0UserId = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                Email = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                FirstName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                LastName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                DisplayName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                Metadata = table.Column<string>(type: "nvarchar(max)", nullable: false),
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_User", x => x.InternalId);
            });

        migrationBuilder.CreateIndex(
            name: "IX_User_Auth0UserId",
            table: "User",
            column: "Auth0UserId",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_User_Email",
            table: "User",
            column: "Email",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_User_Id",
            table: "User",
            column: "Id",
            unique: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "User");
    }
}
