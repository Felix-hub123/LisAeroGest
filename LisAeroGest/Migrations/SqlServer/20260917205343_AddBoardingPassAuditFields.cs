using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LisAeroGest.Migrations.SqlServer
{
    /// <inheritdoc />
    public partial class AddBoardingPassAuditFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "IssuedByName",
                table: "BoardingPasses",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IssuedByUserId",
                table: "BoardingPasses",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IssuedByName",
                table: "BoardingPasses");

            migrationBuilder.DropColumn(
                name: "IssuedByUserId",
                table: "BoardingPasses");
        }
    }
}
