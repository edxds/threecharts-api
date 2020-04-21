using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using ThreeChartsAPI.Features.Charts.Models;

namespace ThreeChartsAPI.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:chart_entry_stat", "new,reentry,no_diff,increase,decrease")
                .Annotation("Npgsql:Enum:chart_entry_type", "album,artist,track");

            migrationBuilder.CreateTable(
                name: "albums",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    title = table.Column<string>(nullable: false),
                    artist_name = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_albums", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "artists",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_artists", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "tracks",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    title = table.Column<string>(nullable: false),
                    artist_name = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_tracks", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    lastfm_id = table.Column<int>(nullable: false),
                    display_name = table.Column<string>(nullable: false),
                    real_name = table.Column<string>(nullable: true),
                    last_fm_url = table.Column<string>(nullable: true),
                    profile_picture = table.Column<string>(nullable: true),
                    registered_at = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_users", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "chart_weeks",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    week_number = table.Column<int>(nullable: false),
                    from = table.Column<DateTime>(nullable: false),
                    to = table.Column<DateTime>(nullable: false),
                    owner_id = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_chart_weeks", x => x.id);
                    table.ForeignKey(
                        name: "fk_chart_weeks_users_owner_id",
                        column: x => x.owner_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "chart_entries",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    rank = table.Column<int>(nullable: false),
                    type = table.Column<ChartEntryType>(nullable: false),
                    stat = table.Column<ChartEntryStat>(nullable: false),
                    stat_text = table.Column<string>(nullable: false),
                    track_id = table.Column<int>(nullable: true),
                    album_id = table.Column<int>(nullable: true),
                    artist_id = table.Column<int>(nullable: true),
                    week_id = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_chart_entries", x => x.id);
                    table.ForeignKey(
                        name: "fk_chart_entries_albums_album_id",
                        column: x => x.album_id,
                        principalTable: "albums",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_chart_entries_artists_artist_id",
                        column: x => x.artist_id,
                        principalTable: "artists",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_chart_entries_tracks_track_id",
                        column: x => x.track_id,
                        principalTable: "tracks",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_chart_entries_chart_weeks_week_id",
                        column: x => x.week_id,
                        principalTable: "chart_weeks",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

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

            migrationBuilder.CreateIndex(
                name: "ix_chart_entries_week_id",
                table: "chart_entries",
                column: "week_id");

            migrationBuilder.CreateIndex(
                name: "ix_chart_weeks_owner_id",
                table: "chart_weeks",
                column: "owner_id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "chart_entries");

            migrationBuilder.DropTable(
                name: "albums");

            migrationBuilder.DropTable(
                name: "artists");

            migrationBuilder.DropTable(
                name: "tracks");

            migrationBuilder.DropTable(
                name: "chart_weeks");

            migrationBuilder.DropTable(
                name: "users");
        }
    }
}
