using Microsoft.EntityFrameworkCore.Migrations;

namespace ThreeChartsAPI.Migrations
{
    public partial class UserAddIanaTimezone : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "iana_timezone",
                table: "users",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "iana_timezone",
                table: "users");
        }
    }
}
