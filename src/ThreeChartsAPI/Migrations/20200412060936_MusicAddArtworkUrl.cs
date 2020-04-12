using Microsoft.EntityFrameworkCore.Migrations;

namespace ThreeChartsAPI.Migrations
{
    public partial class MusicAddArtworkUrl : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "artwork_url",
                table: "tracks",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "artwork_url",
                table: "artists",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "artwork_url",
                table: "albums",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "artwork_url",
                table: "tracks");

            migrationBuilder.DropColumn(
                name: "artwork_url",
                table: "artists");

            migrationBuilder.DropColumn(
                name: "artwork_url",
                table: "albums");
        }
    }
}
