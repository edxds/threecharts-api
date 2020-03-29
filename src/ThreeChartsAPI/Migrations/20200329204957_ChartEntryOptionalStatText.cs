using Microsoft.EntityFrameworkCore.Migrations;

namespace ThreeChartsAPI.Migrations
{
    public partial class ChartEntryOptionalStatText : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "stat_text",
                table: "chart_entries",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "stat_text",
                table: "chart_entries",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldNullable: true);
        }
    }
}
