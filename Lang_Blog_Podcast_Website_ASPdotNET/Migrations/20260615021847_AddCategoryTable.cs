using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lang_Blog_Podcast_Website_ASPdotNET.Migrations
{
    /// <inheritdoc />
    public partial class AddCategoryTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Category",
                table: "Stories");

            migrationBuilder.AddColumn<int>(
                name: "CategoryId",
                table: "Stories",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Stories_CategoryId",
                table: "Stories",
                column: "CategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_Stories_Categories_CategoryId",
                table: "Stories",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Stories_Categories_CategoryId",
                table: "Stories");

            migrationBuilder.DropTable(
                name: "Categories");

            migrationBuilder.DropIndex(
                name: "IX_Stories_CategoryId",
                table: "Stories");

            migrationBuilder.DropColumn(
                name: "CategoryId",
                table: "Stories");

            migrationBuilder.AddColumn<string>(
                name: "Category",
                table: "Stories",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
