using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Loco1.Data.Migrations
{
    /// <inheritdoc />
    public partial class Add_DateDeleted_And_DeletedBy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DateDeleted",
                table: "ShiftWorks",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "ShiftWorks",
                type: "character varying(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DateDeleted",
                table: "Locomotives",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "Locomotives",
                type: "character varying(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DateDeleted",
                table: "Fuels",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "Fuels",
                type: "character varying(256)",
                maxLength: 256,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DateDeleted",
                table: "ShiftWorks");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "ShiftWorks");

            migrationBuilder.DropColumn(
                name: "DateDeleted",
                table: "Locomotives");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "Locomotives");

            migrationBuilder.DropColumn(
                name: "DateDeleted",
                table: "Fuels");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "Fuels");
        }
    }
}
