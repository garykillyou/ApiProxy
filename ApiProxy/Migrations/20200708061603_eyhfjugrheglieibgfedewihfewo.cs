using Microsoft.EntityFrameworkCore.Migrations;

namespace ApiProxy.Migrations
{
    public partial class eyhfjugrheglieibgfedewihfewo : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UrlReferenceDescription",
                table: "AskUrlReferences",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UrlReferenceDescription",
                table: "AskUrlReferences");
        }
    }
}
