using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lang_Blog_Podcast_Website_ASPdotNET.Migrations
{
    /// <inheritdoc />
    public partial class AddMagazineSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MagazineIssues",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IssueNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CoverImagePath = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Season = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    PublishDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EditorId = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MagazineIssues", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MagazineIssues_AspNetUsers_EditorId",
                        column: x => x.EditorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "MagazineArticles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IssueId = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ImagePath = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LayoutType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ViewCount = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CategoryId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MagazineArticles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MagazineArticles_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MagazineArticles_MagazineIssues_IssueId",
                        column: x => x.IssueId,
                        principalTable: "MagazineIssues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MagazineArticles_CategoryId",
                table: "MagazineArticles",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_MagazineArticles_IssueId",
                table: "MagazineArticles",
                column: "IssueId");

            migrationBuilder.CreateIndex(
                name: "IX_MagazineIssues_EditorId",
                table: "MagazineIssues",
                column: "EditorId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MagazineArticles");

            migrationBuilder.DropTable(
                name: "MagazineIssues");
        }
    }
}
