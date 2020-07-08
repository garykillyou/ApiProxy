using Microsoft.EntityFrameworkCore.Migrations;

namespace ApiProxy.Migrations
{
    public partial class eyhfjugrhegli : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "UrlReferences",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "UrlReferences");
        }
    }
}
