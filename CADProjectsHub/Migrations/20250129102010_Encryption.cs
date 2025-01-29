using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CADProjectsHub.Migrations
{
    /// <inheritdoc />
    public partial class Encryption : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "IVKey",
                table: "CADModel",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IVKey",
                table: "CADModel");
        }
    }
}
