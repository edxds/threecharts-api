using Microsoft.EntityFrameworkCore.Migrations;

namespace ThreeChartsAPI.Migrations
{
    public partial class RevampChartSystem : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            /* Clear table data */
            migrationBuilder.Sql("DELETE FROM artists");
            migrationBuilder.Sql("DELETE FROM albums");
            migrationBuilder.Sql("DELETE FROM tracks");
            
            migrationBuilder.DropForeignKey(
                name: "fk_chart_entries_albums_album_id",
                table: "chart_entries");

            migrationBuilder.DropForeignKey(
                name: "fk_chart_entries_artists_artist_id",
                table: "chart_entries");

            migrationBuilder.DropForeignKey(
                name: "fk_chart_entries_tracks_track_id",
                table: "chart_entries");

            migrationBuilder.DropIndex(
                name: "ix_chart_entries_album_id",
                table: "chart_entries");

            migrationBuilder.DropIndex(
                name: "ix_chart_entries_artist_id",
                table: "chart_entries");

            migrationBuilder.DropIndex(
                name: "ix_chart_entries_track_id",
                table: "chart_entries");

            migrationBuilder.DropColumn(
                name: "album_id",
                table: "chart_entries");

            migrationBuilder.DropColumn(
                name: "artist_id",
                table: "chart_entries");

            migrationBuilder.DropColumn(
                name: "track_id",
                table: "chart_entries");

            migrationBuilder.AddColumn<string>(
                name: "artist",
                table: "chart_entries",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "title",
                table: "chart_entries",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "artist",
                table: "chart_entries");

            migrationBuilder.DropColumn(
                name: "title",
                table: "chart_entries");

            migrationBuilder.AddColumn<int>(
                name: "album_id",
                table: "chart_entries",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "artist_id",
                table: "chart_entries",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "track_id",
                table: "chart_entries",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_chart_entries_album_id",
                table: "chart_entries",
                column: "album_id");

            migrationBuilder.CreateIndex(
                name: "ix_chart_entries_artist_id",
                table: "chart_entries",
                column: "artist_id");

            migrationBuilder.CreateIndex(
                name: "ix_chart_entries_track_id",
                table: "chart_entries",
                column: "track_id");

            migrationBuilder.AddForeignKey(
                name: "fk_chart_entries_albums_album_id",
                table: "chart_entries",
                column: "album_id",
                principalTable: "albums",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_chart_entries_artists_artist_id",
                table: "chart_entries",
                column: "artist_id",
                principalTable: "artists",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_chart_entries_tracks_track_id",
                table: "chart_entries",
                column: "track_id",
                principalTable: "tracks",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
