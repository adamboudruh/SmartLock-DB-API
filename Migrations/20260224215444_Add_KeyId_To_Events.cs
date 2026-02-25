using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartLockDBAPI.Migrations
{
    /// <inheritdoc />
    public partial class Add_KeyId_To_Events : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "LastUsed",
                table: "Keys",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<Guid>(
                name: "KeyId",
                table: "Events",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Events_KeyId",
                table: "Events",
                column: "KeyId");

            migrationBuilder.AddForeignKey(
                name: "FK_Events_Keys_KeyId",
                table: "Events",
                column: "KeyId",
                principalTable: "Keys",
                principalColumn: "KeyId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Events_Keys_KeyId",
                table: "Events");

            migrationBuilder.DropIndex(
                name: "IX_Events_KeyId",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "LastUsed",
                table: "Keys");

            migrationBuilder.DropColumn(
                name: "KeyId",
                table: "Events");
        }
    }
}
