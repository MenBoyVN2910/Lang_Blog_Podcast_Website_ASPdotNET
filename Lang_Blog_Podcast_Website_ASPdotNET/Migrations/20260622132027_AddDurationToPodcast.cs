using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lang_Blog_Podcast_Website_ASPdotNET.Migrations
{
    /// <inheritdoc />
    public partial class AddDurationToPodcast : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Duration",
                table: "PodCasts",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Duration",
                table: "PodCasts");
        }
    }
}
