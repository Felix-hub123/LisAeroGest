using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LisAeroGest.Data.Migrations.Postgres
{
    public partial class PassengerPhoneAndDownloadToken : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PhoneNumber",
                table: "Passengers",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DownloadToken",
                table: "Tickets",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "DownloadToken", table: "Tickets");
            migrationBuilder.DropColumn(name: "PhoneNumber", table: "Passengers");
        }
    }
}