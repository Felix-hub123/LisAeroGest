using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LisAeroGest.Migrations.SqlServer
{
    /// <inheritdoc />
    public partial class AddTicketDownloadToken : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DownloadToken",
                table: "Tickets",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DownloadToken",
                table: "Tickets");
        }
    }
}
