using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Loco1.Data.Migrations
    {
    public partial class AddFirstAndLastNameToUsers : Migration
        {
        protected override void Up(MigrationBuilder migrationBuilder)
            {
            migrationBuilder.AddColumn<string>(
                name: "FirstName",
                table: "AspNetUsers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastName",
                table: "AspNetUsers",
                type: "text",
                nullable: true);
            }

        protected override void Down(MigrationBuilder migrationBuilder)
            {
            migrationBuilder.DropColumn(name: "FirstName", table: "AspNetUsers");
            migrationBuilder.DropColumn(name: "LastName", table: "AspNetUsers");
            }
        }
    }