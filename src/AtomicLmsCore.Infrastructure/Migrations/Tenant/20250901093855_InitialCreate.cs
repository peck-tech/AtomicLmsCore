#nullable disable

using Microsoft.EntityFrameworkCore.Migrations;

namespace AtomicLmsCore.Infrastructure.Migrations.Tenant;

/// <inheritdoc />
public partial class InitialCreate : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            "User",
            table => new
            {
                InternalId = table.Column<int>("int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                Auth0UserId = table.Column<string>("nvarchar(255)", maxLength: 255, nullable: false),
                Email = table.Column<string>("nvarchar(255)", maxLength: 255, nullable: false),
                FirstName = table.Column<string>("nvarchar(100)", maxLength: 100, nullable: false),
                LastName = table.Column<string>("nvarchar(100)", maxLength: 100, nullable: false),
                DisplayName = table.Column<string>("nvarchar(200)", maxLength: 200, nullable: false),
                IsActive = table.Column<bool>("bit", nullable: false, defaultValue: true),
                Metadata = table.Column<string>("nvarchar(max)", nullable: false),
                Id = table.Column<Guid>("uniqueidentifier", nullable: false),
                CreatedAt = table.Column<DateTime>("datetime2", nullable: false),
                UpdatedAt = table.Column<DateTime>("datetime2", nullable: false),
                CreatedBy = table.Column<string>("nvarchar(max)", nullable: false),
                UpdatedBy = table.Column<string>("nvarchar(max)", nullable: false),
                IsDeleted = table.Column<bool>("bit", nullable: false, defaultValue: false),
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_User", x => x.InternalId);
            });

        migrationBuilder.CreateIndex(
            "IX_User_Auth0UserId",
            "User",
            "Auth0UserId",
            unique: true);

        migrationBuilder.CreateIndex(
            "IX_User_Email",
            "User",
            "Email",
            unique: true);

        migrationBuilder.CreateIndex(
            "IX_User_Id",
            "User",
            "Id",
            unique: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
        => migrationBuilder.DropTable(name: "User");
}
