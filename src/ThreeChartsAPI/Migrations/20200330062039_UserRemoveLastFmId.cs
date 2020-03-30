using Microsoft.EntityFrameworkCore.Migrations;

namespace ThreeChartsAPI.Migrations
{
    public partial class UserRemoveLastFmId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "lastfm_id",
                table: "users");

            migrationBuilder.RenameColumn("display_name", "users", "user_name");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn("user_name", "users", "display_name");

            migrationBuilder.AddColumn<int>(
                name: "lastfm_id",
                table: "users",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}
