using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AtomicLmsCore.Infrastructure.Migrations.Tenant;

/// <inheritdoc />
public partial class AddLearningObjects : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "LearningObject",
            columns: table => new
            {
                InternalId = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                Name = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                Metadata = table.Column<string>(type: "nvarchar(max)", nullable: false),
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                CreatedBy = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                UpdatedBy = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_LearningObject", x => x.InternalId);
            });

        migrationBuilder.CreateIndex(
            name: "IX_LearningObject_Id",
            table: "LearningObject",
            column: "Id",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_LearningObject_Name",
            table: "LearningObject",
            column: "Name");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "LearningObject");
    }
}
