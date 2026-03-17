using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SmartLockDBAPI.Migrations
{
    /// <inheritdoc />
    public partial class Add_Settings_And_DeviceSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Settings",
                columns: table => new
                {
                    SettingId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DefaultValue = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Settings", x => x.SettingId);
                });

            migrationBuilder.CreateTable(
                name: "DeviceSettings",
                columns: table => new
                {
                    DeviceSettingId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DeviceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SettingId = table.Column<int>(type: "int", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeviceSettings", x => x.DeviceSettingId);
                    table.ForeignKey(
                        name: "FK_DeviceSettings_Devices_DeviceId",
                        column: x => x.DeviceId,
                        principalTable: "Devices",
                        principalColumn: "DeviceId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DeviceSettings_Settings_SettingId",
                        column: x => x.SettingId,
                        principalTable: "Settings",
                        principalColumn: "SettingId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Settings",
                columns: new[] { "SettingId", "DefaultValue", "Name" },
                values: new object[,]
                {
                    { 1, "1", "AutoLockEnabled" },
                    { 2, "15", "AutoLockDelaySec" },
                    { 3, "1", "DoorOpenBuzzEnabled" },
                    { 4, "60", "DoorOpenBuzzDelaySec" },
                    { 5, "1", "OpenCloseToneId" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_DeviceSettings_DeviceId_SettingId",
                table: "DeviceSettings",
                columns: new[] { "DeviceId", "SettingId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DeviceSettings_SettingId",
                table: "DeviceSettings",
                column: "SettingId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DeviceSettings");

            migrationBuilder.DropTable(
                name: "Settings");
        }
    }
}
