using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CADProjectsHub.Migrations
{
    /// <inheritdoc />
    public partial class RenameIVKeyToConstructorInitializationVector : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "IVKey",
                table: "CADModel",
                newName: "ConstructorInitializationVector");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ConstructorInitializationVector",
                table: "CADModel",
                newName: "IVKey");
        }
    }
}
