using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LisAeroGest.Migrations.SqlServer
{
    /// <inheritdoc />
    public partial class PassengerPhone : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PhoneNumber",
                table: "Passengers",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PhoneNumber",
                table: "Passengers");
        }
    }
}
