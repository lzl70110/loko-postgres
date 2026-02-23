using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Loco1.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSoftDeleteFieldsToAspNetUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsDeactivated",
                table: "AspNetUsers",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "OriginalEmail",
                table: "AspNetUsers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OriginalUserName",
                table: "AspNetUsers",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsDeactivated",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "OriginalEmail",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "OriginalUserName",
                table: "AspNetUsers");
        }
    }
}
