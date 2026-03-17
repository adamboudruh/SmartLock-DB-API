using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartLockDBAPI.Migrations
{
    /// <inheritdoc />
    public partial class Add_KeyColor_Column : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Color",
                table: "Keys",
                type: "nvarchar(7)",
                maxLength: 7,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Color",
                table: "Keys");
        }
    }
}
