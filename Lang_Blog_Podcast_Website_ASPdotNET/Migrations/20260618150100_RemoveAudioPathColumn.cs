using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lang_Blog_Podcast_Website_ASPdotNET.Migrations
{
    /// <inheritdoc />
    public partial class RemoveAudioPathColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AudioPath",
                table: "Stories");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AudioPath",
                table: "Stories",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
