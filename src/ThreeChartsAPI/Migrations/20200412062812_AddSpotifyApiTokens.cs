using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ThreeChartsAPI.Migrations
{
    public partial class AddSpotifyApiTokens : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "spotify_api_tokens",
                columns: table => new
                {
                    token = table.Column<string>(nullable: false),
                    type = table.Column<string>(nullable: false),
                    expires_at = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_spotify_api_tokens", x => x.token);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "spotify_api_tokens");
        }
    }
}
